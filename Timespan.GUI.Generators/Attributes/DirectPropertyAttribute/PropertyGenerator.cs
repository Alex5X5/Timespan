namespace Timespan.GUI.Generators.Attributes;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Incremental Roslyn source generator.
///
/// For every field annotated with <c>[BasicDirectProperty&lt;TOwner&gt;]</c> it
/// produces a partial class that contains:
///   • A static <c>DirectProperty&lt;TOwner, TValue&gt;</c> field
///   • A public CLR property that uses <c>SetAndRaise</c>
/// </summary>
[Generator]
public sealed class BasicDirectPropertyGenerator : IIncrementalGenerator {
	// ── Attribute short name (without generic arity) used for quick pre-filter
	private const string AttributeShortName = "BasicDirectProperty";

	public void Initialize(IncrementalGeneratorInitializationContext context) {
		IncrementalValuesProvider<PropertyModel?> models =
			context.SyntaxProvider
				.CreateSyntaxProvider(
					predicate: static (node, _) => IsAnnotatedField(node),
					transform: static (ctx, _) => TransformField(ctx))
				.Where(static m => m is not null);

		context.RegisterSourceOutput(
			models.Collect(),
			static (spc, allModels) => Execute(spc, allModels!));
	}

	private static bool IsAnnotatedField(SyntaxNode node) {
		if (node is not FieldDeclarationSyntax field)
			return false;

		return field.AttributeLists
			.SelectMany(al => al.Attributes)
			.Any(a => a.Name.ToString().Contains(AttributeShortName));
	}

	private static PropertyModel? TransformField(GeneratorSyntaxContext ctx) {
		var fieldDecl = (FieldDeclarationSyntax)ctx.Node;

		// Find the attribute via symbol to confirm it's exactly our type
		foreach (var variable in fieldDecl.Declaration.Variables) {
			var symbol = ctx.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
			if (symbol is null)
				continue;

			var attr = symbol.GetAttributes().FirstOrDefault(
				a => a.AttributeClass?.ConstructedFrom.ToDisplayString()
						 .StartsWith("Timespan.GUI.Generators.Attributes.BasicDirectPropertyAttribute") == true);

			if (attr is null)
				continue;

			// ── Containing class ──────────────────────────────────────────────
			var containingType = symbol.ContainingType;
			if (containingType is null)
				continue;

			// Walk up to collect nested class names (outermost first)
			var nestingChain = new List<string>();
			var cursor = containingType.ContainingType;
			while (cursor is not null) {
				nestingChain.Insert(0, cursor.Name);
				cursor = cursor.ContainingType;
			}

			// ── Field type ────────────────────────────────────────────────────
			string fieldType = symbol.Type.ToDisplayString(
				SymbolDisplayFormat.MinimallyQualifiedFormat);

			// ── Default value (initialiser) ───────────────────────────────────
			string? defaultValue = null;
			var initializer = variable.Initializer?.Value;
			if (initializer is not null)
				defaultValue = initializer.ToString();

			return new PropertyModel(
				Namespace: containingType.ContainingNamespace?.IsGlobalNamespace == true
										  ? null
										  : containingType.ContainingNamespace?.ToDisplayString(),
				ClassName: containingType.Name,
				ContainingTypeNames: nestingChain.ToArray(),
				FieldName: symbol.Name,
				FieldType: fieldType,
				DefaultValueSource: defaultValue);
		}

		return null;
	}
	private static void Execute(
		SourceProductionContext spc,
		ImmutableArray<PropertyModel> allModels) {
		if (allModels.IsDefaultOrEmpty)
			return;

		// Group by the fully-qualified class key so we emit one file per class
		var groups = allModels
			.GroupBy(m => BuildClassKey(m));

		foreach (var group in groups) {
			var first = group.First();

			string source = CodeEmitter.Emit(
				first.Namespace,
				first.ClassName,
				first.ContainingTypeNames,
				group);

			// Hint name: unique per class to avoid collisions
			string hintName = BuildHintName(first);

			spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
		}
	}
	
	private static string BuildClassKey(PropertyModel m) {
		var sb = new StringBuilder();
		if (m.Namespace is not null)
			sb.Append(m.Namespace).Append('.');
		foreach (var outer in m.ContainingTypeNames)
			sb.Append(outer).Append('+');
		sb.Append(m.ClassName);
		return sb.ToString();
	}

	private static string BuildHintName(PropertyModel m) {
		// e.g. "MyNamespace.MyControl.DirectProperties.g.cs"
		var sb = new StringBuilder();
		if (m.Namespace is not null)
			sb.Append(m.Namespace).Append('.');
		foreach (var outer in m.ContainingTypeNames)
			sb.Append(outer).Append('.');
		sb.Append(m.ClassName).Append(".DirectProperties.g.cs");
		return sb.ToString();
	}
}