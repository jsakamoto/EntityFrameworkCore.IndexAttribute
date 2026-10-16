using EntityFrameworkCore.IndexAttributeTest.Models.CompositeOptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(PostgreSqlCollection))]
public class IndexAttributeTest_CompositeOptions_on_PostgreSQL
{
    private readonly PostgreSqlFixture _postgreSql;

    public IndexAttributeTest_CompositeOptions_on_PostgreSQL(PostgreSqlFixture postgreSql)
    {
        this._postgreSql = postgreSql;
    }

    [Fact(DisplayName = "IsUnique on any column of a multi-column index applies to the whole index on PostgreSQL")]
    public void CreateDb_with_IsUnique_On_Any_Column_Test()
    {
        var options = new DbContextOptionsBuilder()
            .UseNpgsql(this._postgreSql.CreateDatabase())
            .Options;
        using var db = new CompositeOptionsDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                        SELECT indexdef FROM pg_indexes
                        WHERE schemaname = 'public' AND tablename IN ('Articles', 'Visits') AND indexname NOT IN (SELECT conname FROM pg_constraint WHERE contype = 'p')
                        ORDER BY tablename, indexname COLLATE ""C""";
            var dump = new List<string>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) dump.Add(r.GetString(0));
            }
            dump.Is(
                @"CREATE UNIQUE INDEX ""IX_Tenant_Slug"" ON public.""Articles"" USING btree (""TenantId"", ""Slug"")",
                @"CREATE INDEX ""IX_Clustered"" ON public.""Visits"" USING btree (""PageId"", ""Sequence"")",
                @"CREATE INDEX ""IX_Includes"" ON public.""Visits"" USING btree (""CampaignId"", ""SourceId"")",
                @"CREATE UNIQUE INDEX ""IX_Unique"" ON public.""Visits"" USING btree (""SiteId"", ""VisitorId"")");
        }
        finally
        {
            // PostgreSQL can't drop a database while a connection to it is open.
            db.Database.CloseConnection();
            db.Database.EnsureDeleted();
        }
    }
}
