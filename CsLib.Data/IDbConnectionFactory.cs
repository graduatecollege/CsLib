using System.Data.Common;

namespace Grad.CsLib.Data;

/// <summary>
/// Defines a factory for creating and managing database connections asynchronously.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    /// <remarks>
    /// The caller is responsible for closing the connection. The recommended
    /// pattern is to use `await using` to ensure the connection is closed.
    /// </remarks>
    Task<DbConnection> ConnectAsync();
}