using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Grad.CsLib.Data;

//.AddSingleton<IDbConnectionFactory>(s => new DbConnectionFactory(builder.Configuration.GetConnectionString("Certification")!));

/// <summary>
/// A factory for creating and managing SQL database connections asynchronously.
/// </summary>
/// <remarks>
/// Register the factory as a singleton in DI:
/// <code>
/// services.AddSingleton&lt;IDbConnectionFactory>(s => new DbConnectionFactory(builder.Configuration.GetConnectionString("DbName")!));
/// </code>
///
/// Inject it to your service:
/// <code>
/// public class MyService(IDbConnectionFactory connectionFactory)
/// </code>
///
/// And then use it in a method:
/// <code>
/// await using var connection = await connectionFactory.ConnectAsync();
/// </code>
/// 
/// </remarks>
public class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<DbConnection> ConnectAsync()
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}