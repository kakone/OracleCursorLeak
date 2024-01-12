using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;

namespace OracleCursorLeak;

internal class Program
{
    private static void Main()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        Console.WriteLine(GetOpenedCursorsText(serviceProvider));

        Console.WriteLine("Updates...");

        while (UpdateNumbers(serviceProvider) == 0)
        {
            DataSeeding(serviceProvider);
        }

        Console.WriteLine(GetOpenedCursorsText(serviceProvider));

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.WriteLine();

        Console.WriteLine(GetOpenedCursorsText(serviceProvider));
        Console.WriteLine("The only way to close the cursors is to clear the connection pool...");
        ClearPool(serviceProvider);
        Console.WriteLine(GetOpenedCursorsText(serviceProvider));

        Console.WriteLine();
        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>();
    }

    private static void DataSeeding(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        for (var i = 0; i < 1000; i++)
        {
            dbContext.Tests.Add(new Test());
        }
        dbContext.SaveChanges();
    }

    private static void ClearPool(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        OracleConnection.ClearPool((OracleConnection)dbContext.Database.GetDbConnection());
    }

    private static int UpdateNumbers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var tests = dbContext.Tests.Take(100).ToList();
        foreach (var test in tests)
        {
            test.Number = Random.Shared.Next();
        }
        dbContext.SaveChanges();
        return tests.Count;
    }

    private static string GetOpenedCursorsText(IServiceProvider serviceProvider)
    {
        return $"Opened cursors : {GetOpenedCursors(serviceProvider)}";
    }

    private static int GetOpenedCursors(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var openedCursorsSQL = $"SELECT COUNT(*) \"Value\" FROM v$open_cursor WHERE user_name = '{MyDbContext.USER_ID.ToUpperInvariant()}'";
        return dbContext.Database.SqlQueryRaw<int>(openedCursorsSQL).First();
    }
}
