using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace OracleCursorLeak;

internal class MyDbContext : DbContext
{
    public const string USER_ID = "userId";
    private const string CONNECTION_STRING = $"Data Source=127.0.0.1:1521/ORCL;User Id={USER_ID};Password=password;";

    public DbSet<Test> Tests { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseOracle(CONNECTION_STRING);

#if DEBUG
        options.LogTo(message => Debug.WriteLine(message))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>();
    }
}
