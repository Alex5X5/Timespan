namespace DatabaseUtil;

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

public class DatabaseAccessor<DbContextType> where DbContextType : DbContext {

	private readonly DbContextType? _context;
	private readonly Dictionary<Type, PropertyInfo?> primaryKeyProperties = GetPrimaryKeyProperties();

	public DatabaseAccessor(string? path, DatabasePathFormat pathFormat, ConfigurationManager? config) {
		var optionsBuilder = new DbContextOptionsBuilder<DbContextType>();
		string connectionString = pathFormat switch {
			DatabasePathFormat.ConnectionString => path,
			DatabasePathFormat.FileName => "Data Source=" + path,
			DatabasePathFormat.ReadConfig => config?.GetConnectionString("DefaultConnection"),
			_ => ""
		} ?? "";
		optionsBuilder.UseSqlite(connectionString);
		_context = (DbContextType)BuildContext(optionsBuilder.Options);
		//_context.Database.EnsureCreated();
		_context.Database.Migrate();
	}

	private static DbContext BuildContext(DbContextOptions options) {
		var constructor = typeof(DbContextType).GetConstructor([typeof(DbContextOptions)])
			?? throw new NotSupportedException($"found no constructor for the type {typeof(DbContextType)} taking DbContextOptions while building DbContext");
		return (DbContext)constructor.Invoke([options]);
	}

	private static PropertyInfo? GetModelTypePrimaryKeyProperty(Type modelType) {
		foreach (PropertyInfo property in modelType.GetProperties()) {
			IEnumerable<Attribute> genericTypeProperties = property.GetCustomAttributes();
			foreach (Attribute annotation in genericTypeProperties) {
				bool isGenerated = annotation.GetType() == typeof(DatabaseGeneratedAttribute);
				if (!isGenerated)
					continue;
				bool isIdentity = ((DatabaseGeneratedAttribute)annotation).DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
				if (isGenerated && isIdentity) {
					return property;
				}
			}
		}
		return null;
	}

	private static Dictionary<Type, PropertyInfo?> GetPrimaryKeyProperties() {
		Dictionary<Type, PropertyInfo?> keys = [];
		List<PropertyInfo> dbSets = typeof(DbContextType)
			.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(p => p.PropertyType.IsGenericType &&
							p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
					.ToList();
		foreach (var set in dbSets) {
			var setType = set.PropertyType;
			Type setEntityType = setType.GetGenericArguments()[0];
			keys[setEntityType] = GetModelTypePrimaryKeyProperty(setEntityType);
			if (keys[setEntityType]==null)
				throw new NotSupportedException($"the entity type of the DbSet {setEntityType} does not contain a property that is annotated as a primary key");
		}
		return keys;
	}

	private static object? ExpandoToDynamicType(ExpandoObject expando) {
		var expandoDict = (IDictionary<string, object?>)expando;
		var assemblyName = new AssemblyName("DynamicAnonTypes");
		var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
		var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
		var typeBuilder = moduleBuilder.DefineType(
			"AnonType" + Guid.NewGuid().ToString("N"),
			TypeAttributes.Public | TypeAttributes.Class);

		foreach (var kvp in expandoDict) {
			var field = typeBuilder.DefineField("_" + kvp.Key, kvp.Value?.GetType() ?? typeof(object), FieldAttributes.Private);
			var property = typeBuilder.DefineProperty(kvp.Key, PropertyAttributes.HasDefault, kvp.Value?.GetType() ?? typeof(object), null);

			var getter = typeBuilder.DefineMethod(
				"get_" + kvp.Key,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				kvp.Value?.GetType() ?? typeof(object),
				Type.EmptyTypes);

			var getterIL = getter.GetILGenerator();
			getterIL.Emit(OpCodes.Ldarg_0);
			getterIL.Emit(OpCodes.Ldfld, field);
			getterIL.Emit(OpCodes.Ret);

			var setter = typeBuilder.DefineMethod(
				"set_" + kvp.Key,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				null,
				[kvp.Value?.GetType() ?? typeof(object)]
			);

			var setterIL = setter.GetILGenerator();
			setterIL.Emit(OpCodes.Ldarg_0);
			setterIL.Emit(OpCodes.Ldarg_1);
			setterIL.Emit(OpCodes.Stfld, field);
			setterIL.Emit(OpCodes.Ret);

			property.SetGetMethod(getter);
			property.SetSetMethod(setter);
		}

		var dynamicType = typeBuilder.CreateType();

		object? instance = Activator.CreateInstance(dynamicType);

		foreach (var kvp in expandoDict) {
			PropertyInfo? prop = dynamicType.GetProperty(kvp.Key);
			prop?.SetValue(instance, kvp.Value);
		}

		return instance;
	}

	private object? PrimVal<T>(T? source) where T : class =>
		primaryKeyProperties.ContainsKey(typeof(T)) ? primaryKeyProperties[typeof(T)]?.GetValue(source) : null;

	private async Task<bool> SaveChangesAsync() {
		if (_context == null)
			return false;
		try {
			await _context.SaveChangesAsync();
		} catch (DbUpdateConcurrencyException) {
			return false;
		}
		return true;
	}

	private bool SaveChangesBlocking() {
		if (_context == null)
			return false;
		try {
			_context.SaveChanges();
		} catch (DbUpdateConcurrencyException) {
			return false;
		}
		return true;
	}

	public bool PrimaryKeyValueExistsInDatabase<EntityType>(object? key) where EntityType : class {
		if (key == null)
			return false;
		if (_context?.Set<EntityType>().Find(key) != null)
			return true;
		return false;
	}

	public bool PrimaryKeyOfEntryExistsInDatabase<T>(object? key) where T : class =>
		PrimaryKeyValueExistsInDatabase<T>(PrimVal<T>((T)key));

	public async Task<T?> QuerySingleByKeyAsync<T>(object primaryKeyValue) where T : class {
		if (_context == null)
			return null;
		if (primaryKeyProperties.ContainsKey(typeof(T)))
			return await _context.Set<T>().FindAsync(primaryKeyValue);
		return null;
	}

	public T? QuerySingleByKeyBlocking<T>(object keyValue) where T : class {
		if (_context == null)
			return null;
		if (primaryKeyProperties.ContainsKey(typeof(T)))
			_context.Find<DbContextType>(keyValue);
		return null;
	}

	public async Task<IEnumerable<T>?> QueryRangeByPropertiesAsync<T>(object filter) where T : class {
		if (_context == null)
			return null;
		List<PropertyInfo> properties = [.. filter.GetType().GetProperties()];
		IEnumerable<T> items = await QueryAllAsync<T>();
		foreach (PropertyInfo property in properties) {
			items = items.Where(p => property.GetValue(p) == property.GetValue(filter));
		}
		return items;
	}
	
	public async Task<List<T>> QueryAllAsync<T>() where T : class {
		if (_context == null)
			return [];
		return await _context.Set<T>().ToListAsync();
	}

	public IEnumerable<T> QueryAllEnumerable<T>() where T : class {
		return _context?.Set<T>().AsEnumerable() ?? [];
	}

	public List<T> QueryAllBlocking<T>() where T : class =>
		_context != null ? _context.Set<T>().ToList() : [];

	public async Task<bool> AddAsync<T>(T item, bool updateIfExists) where T : class {
		if (_context == null)
			return false;
		if (PrimaryKeyOfEntryExistsInDatabase<T>(item)) {
			if (!updateIfExists)
				return false;
			await UpdateAsync(item, false);
			return await SaveChangesAsync();
		}
		if(await _context.Set<T>().AddAsync(item) == null)
			return false;
		return await SaveChangesAsync();
	}

	public bool AddBlocking<T>(T item, bool updateIfExists) where T : class {
		if (_context == null)
			return false;
		if (!PrimaryKeyOfEntryExistsInDatabase<T>(item)) {
			_context.Set<T>().Add(item);
		} else if (updateIfExists) {
			UpdateBlocking(item, false);
		} else {
			return false;
		}
		try {
			_context.SaveChanges();
		} catch (DbUpdateConcurrencyException) {
			return false;
		}
		return true;
	}

	public async Task<bool> ApplyUpdatesOnSingleAsync<T>(object key, Action<T> updateFunc, bool createIfNotExists = false) where T : class {
		if (_context == null)
			return false;
		T? item = await QuerySingleByKeyAsync<T>(key);
		if (item == null)
			return false;
		updateFunc(item);
		return await SaveChangesAsync();
	}

	public async Task<bool> ApplyUpdatesOnAllAsync<T>(Action<List<T>> updateFunc) where T : class {
		if (_context == null)
			return false;
		List<T> items = await QueryAllAsync<T>();
		updateFunc(items);
		return await SaveChangesAsync();
	}

	private async Task<bool> UpdateAsync<T>(T value, bool createIfNotExists) where T : class {
		if (_context == null)
			return false;
		T? item = await QuerySingleByKeyAsync<T>(value);
		if (item == null) {
			if (createIfNotExists) {
				return await AddAsync<T>(value, false);
			} else {
				return false;
			}
		}
		IEnumerable<PropertyInfo> objectProperties = typeof(T).GetProperties();
		foreach (PropertyInfo p in objectProperties) {
			if (p.Name == primaryKeyProperties[typeof(T)]?.Name)
				continue;
			if (p.IsDefined(typeof(NotMappedAttribute), inherit: true))
				continue;
			p.SetValue(item, p.GetValue(value), null);
		}
		return await SaveChangesAsync();
	}

	public bool UpdateBlocking<T>(T updatedObject, bool createIfNotExists) where T : class =>
		UpdateAsync(updatedObject, createIfNotExists).Result;
	
	public async Task<bool> DeleteAsync<T>(T item) where T : class =>
		await DeleteByKeyAsync<T>(PrimVal(item));

	public async Task<bool> DeleteByKeyAsync<T>(object? key) where T : class {
		if (_context == null)
			return false;
		if (key == null)
			return false;
		if (!PrimaryKeyValueExistsInDatabase<T>(key))
			return false;
		foreach (var it in _context.Set<T>()) {
			if (key.Equals(PrimVal(it))) {
				_context.Set<T>().Remove(it);
				return await SaveChangesAsync();
			}
		}
		return false;
	}

	public bool DeleteBlocking<EntityType>(EntityType item) where EntityType : class =>
		DeleteByKeyBlocking<EntityType>(PrimVal(item));

	public bool DeleteByKeyBlocking<EntityType>(object? key) where EntityType : class {
		if (_context == null)
			return false;
		if (PrimaryKeyValueExistsInDatabase<EntityType>(key)) {
			if (_context.Set<EntityType>().Find(key) == null)
				return false;
			else {
				_context.Set<EntityType>().Remove(
					_context.Set<EntityType>().First(
						x => PrimVal(x) == key
					)
				);
			}
			return true;
		}
		return SaveChangesBlocking();
	}
}


public enum DatabasePathFormat {
	ConnectionString,
	FileName,
	ReadConfig
}