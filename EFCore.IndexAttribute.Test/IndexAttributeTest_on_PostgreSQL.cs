using EntityFrameworkCore.IndexAttributeTest.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

[Collection(nameof(PostgreSqlCollection))]
public class IndexAttributeTest_on_PostgreSQL
{
    private readonly PostgreSqlFixture _postgreSql;

    public IndexAttributeTest_on_PostgreSQL(PostgreSqlFixture postgreSql)
    {
        this._postgreSql = postgreSql;
    }

    [Fact(DisplayName = "CreateDb with Indexes on PostgreSQL")]
    public void CreateDb_with_Indexes_Test()
    {
        var option = new DbContextOptionsBuilder<MyDbContextBase>()
            .UseNpgsql(this._postgreSql.CreateDatabase())
            .Options;
        using var db = new MyDbContext(option);

        // Create database.
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            // Validate database indexes.
            var conn = db.Database.GetDbConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                        SELECT indexdef FROM pg_indexes
                        WHERE schemaname = 'public' AND indexname NOT IN (SELECT conname FROM pg_constraint WHERE contype = 'p')
                        ORDER BY tablename, indexname COLLATE ""C""";
            var dump = new List<string>();
            var r = cmd.ExecuteReader();
            try { while (r.Read()) dump.Add(r.GetString(0)); }
            finally { r.Close(); }
            dump.Is(
                @"CREATE INDEX ""IX_Country"" ON public.""People"" USING btree (""Address_Country"")",
                @"CREATE INDEX ""IX_Lines"" ON public.""People"" USING btree (""Line1"", ""Line2"")",
                @"CREATE INDEX ""IX_People_FaxNumber_CountryCode"" ON public.""People"" USING btree (""FaxNumber_CountryCode"")",
                @"CREATE UNIQUE INDEX ""IX_People_Name"" ON public.""People"" USING btree (""Name"")",
                @"CREATE INDEX ""IX_People_PhoneNumber_CountryCode"" ON public.""People"" USING btree (""PhoneNumber_CountryCode"")",
                @"CREATE INDEX ""IX_SNSAccounts_PersonId"" ON public.""SNSAccounts"" USING btree (""PersonId"")",
                @"CREATE INDEX ""IX_SNSAccounts_Provider"" ON public.""SNSAccounts"" USING btree (""Provider"")",
                @"CREATE UNIQUE INDEX ""Ix_Provider_and_Account"" ON public.""SNSAccounts"" USING btree (""Provider"", ""AccountName"")");
        }
        finally
        {
            // PostgreSQL can't drop a database while a connection to it is open.
            db.Database.CloseConnection();
            db.Database.EnsureDeleted();
        }
    }
}
