using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

/// <summary>
/// Starts a PostgreSQL container once for the tests in the <see cref="PostgreSqlCollection"/> collection, and removes it after they finish.
/// </summary>
public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18").Build();

    public async ValueTask InitializeAsync() => await this._container.StartAsync();

    public ValueTask DisposeAsync() => this._container.DisposeAsync();

    /// <summary>
    /// Creates a new empty database in the container, and returns the connection string to it.
    /// </summary>
    public string CreateDatabase()
    {
        var dbName = Guid.NewGuid().ToString("N");
        var connStrToDefault = this._container.GetConnectionString();

        using (var connToDefault = new NpgsqlConnection(connStrToDefault))
        using (var cmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\"", connToDefault))
        {
            connToDefault.Open();
            cmd.ExecuteNonQuery();
        }

        return new NpgsqlConnectionStringBuilder(connStrToDefault) { Database = dbName }.ConnectionString;
    }
}

/// <summary>
/// Groups the tests that share one <see cref="PostgreSqlFixture"/>.
/// </summary>
[CollectionDefinition(nameof(PostgreSqlCollection))]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>;
