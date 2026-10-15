using EntityFrameworkCore.IndexAttributeTest.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(SqlServerCollection))]
public class IndexAttributeTest
{
    private readonly SqlServerFixture _sqlServer;

    public IndexAttributeTest(SqlServerFixture sqlServer)
    {
        this._sqlServer = sqlServer;
    }

    private MyDbContextBase CreateMyDbContext(bool enableSqlServerFeature)
    {
        var connStr = this._sqlServer.CreateDatabase();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
              .AddDebug()
              .AddFilter(
                category: DbLoggerCategory.Database.Command.Name,
                level: LogLevel.Information);
        });

        var option = new DbContextOptionsBuilder<MyDbContextBase>()
            .UseSqlServer(connStr)
            .UseLoggerFactory(loggerFactory)
            .Options;

        return enableSqlServerFeature ?
            new MyDbContextForSqlServer(option) as MyDbContextBase :
            new MyDbContext(option);
    }

    [Fact(DisplayName = "CreateDb with Indexes")]
    public void CreateDb_with_Indexes_Test()
    {
        var dump = this.CreateDbAndDumpIndexes(enableSqlServerFeature: false);
        dump.Is(
            $"People|IX_Country|Address_Country|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_Lines|Line1|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_Lines|Line2|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_FaxNumber_CountryCode|FaxNumber_CountryCode|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_Name|Name|True|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_PhoneNumber_CountryCode|PhoneNumber_CountryCode|False|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|Ix_Provider_and_Account|Provider|True|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|Ix_Provider_and_Account|AccountName|True|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|IX_SNSAccounts_PersonId|PersonId|False|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|IX_SNSAccounts_Provider|Provider|False|NONNULLABLE|NONCLUSTERED|False"
        );
    }

    [Fact(DisplayName = "CreateDb with Indexes for SQL Server")]
    public void CreateDb_with_IndexesForSqlServer_Test()
    {
        var dump = this.CreateDbAndDumpIndexes(enableSqlServerFeature: true);
        dump.Is(
            $"People|IX_Country|Address_Country|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_Country|Address_ZipPostCode|False|NONNULLABLE|NONCLUSTERED|True",
            $"People|IX_Country|Address_TownCity|False|NONNULLABLE|NONCLUSTERED|True",
            $"People|IX_Lines|Line1|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_Lines|Line2|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_FaxNumber_CountryCode|FaxNumber_CountryCode|False|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_Name|Name|True|NONNULLABLE|NONCLUSTERED|False",
            $"People|IX_People_PhoneNumber_CountryCode|PhoneNumber_CountryCode|False|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|Ix_Provider_and_Account|Provider|True|NONNULLABLE|CLUSTERED|False",
            $"SNSAccounts|Ix_Provider_and_Account|AccountName|True|NONNULLABLE|CLUSTERED|False",
            $"SNSAccounts|IX_SNSAccounts_PersonId|PersonId|False|NONNULLABLE|NONCLUSTERED|False",
            $"SNSAccounts|IX_SNSAccounts_Provider|Provider|False|NONNULLABLE|NONCLUSTERED|False"
        );
    }

    private IEnumerable<string> CreateDbAndDumpIndexes(bool enableSqlServerFeature)
    {
        using var db = this.CreateMyDbContext(enableSqlServerFeature);

        // Create database.
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            // Validate database indexes.
            if (!(db.Database.GetDbConnection() is SqlConnection conn)) throw new NullReferenceException("db.Database.GetDbConnection() returns not SqlConnection.");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                        SELECT [Table] = t.name, [Index] = ind.name, [Column] = col.name, [IsUnique] = ind.is_unique, [Nullable] = col.is_nullable, [Type] = ind.type_desc, [IsInclude] = ic.is_included_column
                        FROM sys.indexes ind 
                        INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
                        INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
                        INNER JOIN sys.tables t ON ind.object_id = t.object_id 
                        WHERE ind.is_primary_key = 0 AND t.is_ms_shipped = 0 
                        ORDER BY t.name, ind.name, ind.index_id, ic.is_included_column, ic.key_ordinal;";
            var dump = new List<string>();
            var r = cmd.ExecuteReader();
            try { while (r.Read()) dump.Add($"{r["Table"]}|{r["Index"]}|{r["Column"]}|{r["IsUnique"]}|{((bool)r["Nullable"] ? "NULLABLE" : "NONNULLABLE")}|{r["Type"]}|{r["IsInclude"]}"); }
            finally { r.Close(); }
            return dump;
        }
        finally { db.Database.EnsureDeleted(); }
    }
}
