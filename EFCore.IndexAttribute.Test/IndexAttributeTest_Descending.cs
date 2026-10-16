using EntityFrameworkCore.IndexAttributeTest.Models.Descending;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(SqlServerCollection))]
public class IndexAttributeTest_Descending
{
    private readonly SqlServerFixture _sqlServer;

    public IndexAttributeTest_Descending(SqlServerFixture sqlServer)
    {
        this._sqlServer = sqlServer;
    }

    [Theory(DisplayName = "CreateDb with descending Indexes on SQL Server")]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateDb_with_Descending_Indexes_Test(bool enableSqlServerFeature)
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(this._sqlServer.CreateDatabase())
            .Options;
        using var db = enableSqlServerFeature ? new DescendingDbContextForSqlServer(options) : new DescendingDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                        SELECT [Index] = ind.name, [Column] = col.name, [IsDescending] = ic.is_descending_key
                        FROM sys.indexes ind
                        INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id
                        INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id
                        INNER JOIN sys.tables t ON ind.object_id = t.object_id
                        WHERE t.is_ms_shipped = 0 AND ind.is_primary_key = 0
                        ORDER BY ind.name, ic.key_ordinal;";
            var dump = new List<string>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) dump.Add($"{r["Index"]}|{r["Column"]}|{r["IsDescending"]}");
            }
            dump.Is(
                "IX_Category_Score|Category|False",
                "IX_Category_Score|Score|True",
                "IX_Rankings_CreatedAt|CreatedAt|True");
        }
        finally { db.Database.EnsureDeleted(); }
    }
}
