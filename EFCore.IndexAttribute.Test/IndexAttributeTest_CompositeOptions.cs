using EntityFrameworkCore.IndexAttributeTest.Models.CompositeOptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(SqlServerCollection))]
public class IndexAttributeTest_CompositeOptions
{
    private readonly SqlServerFixture _sqlServer;

    public IndexAttributeTest_CompositeOptions(SqlServerFixture sqlServer)
    {
        this._sqlServer = sqlServer;
    }

    [Fact(DisplayName = "Options on any column of a multi-column index or primary key apply to the whole one on SQL Server")]
    public void CreateDb_with_Options_On_Any_Column_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(this._sqlServer.CreateDatabase())
            .Options;
        using var db = new CompositeOptionsDbContextForSqlServer(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                        SELECT [Table] = t.name, [Index] = ind.name, [Column] = col.name, [IsUnique] = ind.is_unique, [Type] = ind.type_desc, [IsInclude] = ic.is_included_column
                        FROM sys.indexes ind
                        INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id
                        INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id
                        INNER JOIN sys.tables t ON ind.object_id = t.object_id
                        WHERE t.is_ms_shipped = 0
                        ORDER BY t.name, ind.name, ic.is_included_column, ic.key_ordinal, col.name;";
            var dump = new List<string>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) dump.Add($"{r["Table"]}|{r["Index"]}|{r["Column"]}|{r["IsUnique"]}|{r["Type"]}|{r["IsInclude"]}");
            }
            dump.Is(
                "Articles|IX_Tenant_Slug|TenantId|True|CLUSTERED|False",
                "Articles|IX_Tenant_Slug|Slug|True|CLUSTERED|False",
                "Articles|PK_Articles|Id|True|NONCLUSTERED|False",
                "OrderLines|PK_OrderLines|OrderId|True|CLUSTERED|False",
                "OrderLines|PK_OrderLines|LineNumber|True|CLUSTERED|False",
                "Visits|IX_Clustered|PageId|False|CLUSTERED|False",
                "Visits|IX_Clustered|Sequence|False|CLUSTERED|False",
                "Visits|IX_Includes|CampaignId|False|NONCLUSTERED|False",
                "Visits|IX_Includes|SourceId|False|NONCLUSTERED|False",
                "Visits|IX_Includes|Referrer|False|NONCLUSTERED|True",
                "Visits|IX_Includes|UserAgent|False|NONCLUSTERED|True",
                "Visits|IX_Unique|SiteId|True|NONCLUSTERED|False",
                "Visits|IX_Unique|VisitorId|True|NONCLUSTERED|False",
                "Visits|PK_Visits|Id|True|NONCLUSTERED|False");
        }
        finally { db.Database.EnsureDeleted(); }
    }
}
