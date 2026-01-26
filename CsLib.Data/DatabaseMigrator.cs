using System.Reflection;
using DbUp;

namespace Grad.CsLib.Data;

/// <summary>
/// Handles database migrations using DbUp.
/// </summary>
public class DatabaseMigrator
{
    private readonly string _connectionString;

    /// <summary>
    /// Handles database migrations using DbUp and records the migration history into a schema called `DbUp`, which must exist.
    /// </summary>
    /// <param name="connectionString">The connection string to the database.</param>
    public DatabaseMigrator(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Executes the database migration.
    /// </summary>
    /// <returns>True if successful, false otherwise.</returns>
    public (bool, Exception?) Upgrade()
    {
        try
        {
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(_connectionString)
                    .WithTransactionPerScript()
                    .JournalToSqlTable("DbUp", "SchemaVersions")
                    .WithScriptsEmbeddedInAssembly(Assembly.GetCallingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return (false, result.Error);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in upgrader");
            Console.WriteLine(e);
            return (false, e);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();

        return (true, null);
    }
}
