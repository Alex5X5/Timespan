namespace Timespan.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

public class HourglassDbContext : DbContext {

	public DbSet<Models.Task> Tasks { get; set; }
	public DbSet<Models.Project> Projects { get; set; }
	public DbSet<Models.Ticket> Tickets { get; set; }

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
