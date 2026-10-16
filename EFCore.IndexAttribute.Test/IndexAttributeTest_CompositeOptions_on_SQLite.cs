using EntityFrameworkCore.IndexAttributeTest.Models.CompositeOptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

public class IndexAttributeTest_CompositeOptions_on_SQLite
{
    [Fact(DisplayName = "IsUnique on any column of a multi-column index applies to the whole index on SQLite")]
    public void CreateDb_with_IsUnique_On_Any_Column_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new CompositeOptionsDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'index' AND tbl_name IN ('Articles', 'Visits') ORDER BY tbl_name, name";
        var dump = new List<string>();
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read()) dump.Add(r.GetString(0));
        }
        dump.Is(
            @"CREATE UNIQUE INDEX ""IX_Tenant_Slug"" ON ""Articles"" (""TenantId"", ""Slug"")",
            @"CREATE INDEX ""IX_Clustered"" ON ""Visits"" (""PageId"", ""Sequence"")",
            @"CREATE INDEX ""IX_Includes"" ON ""Visits"" (""CampaignId"", ""SourceId"")",
            @"CREATE UNIQUE INDEX ""IX_Unique"" ON ""Visits"" (""SiteId"", ""VisitorId"")");
    }
}
