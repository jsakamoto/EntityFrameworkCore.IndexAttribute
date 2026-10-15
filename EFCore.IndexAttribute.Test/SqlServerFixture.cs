using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

/// <summary>
/// Starts a SQL Server container once for the tests in the <see cref="SqlServerCollection"/> collection, and removes it after they finish.
/// </summary>
public class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();

    public async ValueTask InitializeAsync() => await this._container.StartAsync();

    public ValueTask DisposeAsync() => this._container.DisposeAsync();

    /// <summary>
    /// Creates a new empty database in the container, and returns the connection string to it.
    /// </summary>
    public string CreateDatabase()
    {
        var dbName = Guid.NewGuid().ToString("N");
        var connStrToMaster = this._container.GetConnectionString();

        using (var connToMaster = new SqlConnection(connStrToMaster))
        using (var cmd = new SqlCommand($"CREATE DATABASE [{dbName}]", connToMaster))
        {
            connToMaster.Open();
            cmd.ExecuteNonQuery();
        }

        return new SqlConnectionStringBuilder(connStrToMaster) { InitialCatalog = dbName }.ConnectionString;
    }
}

/// <summary>
/// Groups the tests that share one <see cref="SqlServerFixture"/>.
/// </summary>
[CollectionDefinition(nameof(SqlServerCollection))]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>;
