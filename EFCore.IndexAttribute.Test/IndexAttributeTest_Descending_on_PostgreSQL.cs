using EntityFrameworkCore.IndexAttributeTest.Models.Descending;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(PostgreSqlCollection))]
public class IndexAttributeTest_Descending_on_PostgreSQL
{
    private readonly PostgreSqlFixture _postgreSql;

    public IndexAttributeTest_Descending_on_PostgreSQL(PostgreSqlFixture postgreSql)
    {
        this._postgreSql = postgreSql;
    }

    [Fact(DisplayName = "CreateDb with descending Indexes on PostgreSQL")]
    public void CreateDb_with_Descending_Indexes_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(this._postgreSql.CreateDatabase())
            .Options;
        using var db = new DescendingDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                        SELECT indexdef FROM pg_indexes
                        WHERE schemaname = 'public' AND indexname NOT IN (SELECT conname FROM pg_constraint WHERE contype = 'p')
                        ORDER BY indexname COLLATE ""C""";
            var dump = new List<string>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) dump.Add(r.GetString(0));
            }
            dump.Is(
                @"CREATE INDEX ""IX_Category_Score"" ON public.""Rankings"" USING btree (""Category"", ""Score"" DESC)",
                @"CREATE INDEX ""IX_Rankings_CreatedAt"" ON public.""Rankings"" USING btree (""CreatedAt"" DESC)");
        }
        finally
        {
            // PostgreSQL can't drop a database while a connection to it is open.
            db.Database.CloseConnection();
            db.Database.EnsureDeleted();
        }
    }
}
