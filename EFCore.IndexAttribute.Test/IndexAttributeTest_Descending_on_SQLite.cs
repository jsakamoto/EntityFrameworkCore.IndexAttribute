using EntityFrameworkCore.IndexAttributeTest.Models.Descending;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

public class IndexAttributeTest_Descending_on_SQLite
{
    [Fact(DisplayName = "CreateDb with descending Indexes on SQLite")]
    public void CreateDb_with_Descending_Indexes_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new DescendingDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'index' ORDER BY name";
        var dump = new List<string>();
        using (var r = cmd.ExecuteReader())
        {
            while (r.Read()) dump.Add(r.GetString(0));
        }
        dump.Is(
            @"CREATE INDEX ""IX_Category_Score"" ON ""Rankings"" (""Category"", ""Score"" DESC)",
            @"CREATE INDEX ""IX_Rankings_CreatedAt"" ON ""Rankings"" (""CreatedAt"" DESC)");
    }
}
