namespace Timespan.Database;

using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Types = Timespan.Types.Models;

public class HourglassDbContext : DbContext {

	public DbSet<Types.Task> Tasks { get; set; }
	public DbSet<Types.Project> Projects { get; set; }
	public DbSet<Types.Ticket> Tickets { get; set; }

	public HourglassDbContext(DbContextOptions options) : base(options) {
		foreach (var entityType in Model.GetEntityTypes()) {
			Console.WriteLine($"Entity: {entityType.Name}");
		}
	}
}

public class HourglassDbContextFactory : IDesignTimeDbContextFactory<HourglassDbContext> {
	public HourglassDbContext CreateDbContext(string[] args) {
		var optionsBuilder = new DbContextOptionsBuilder<HourglassDbContext>();
		optionsBuilder.UseSqlite("Data Source=hourglass.db");
		return new HourglassDbContext(optionsBuilder.Options);
	}
}
