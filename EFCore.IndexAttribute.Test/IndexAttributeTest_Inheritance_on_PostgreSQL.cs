using EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

/// <summary>
/// Issue #12. [PrimaryKey] on a base class property must not make EF Core throw "A key cannot be configured on ... because it is a derived type".
/// </summary>
[Collection(nameof(PostgreSqlCollection))]
public class IndexAttributeTest_Inheritance_on_PostgreSQL
{
    private readonly PostgreSqlFixture _postgreSql;

    public IndexAttributeTest_Inheritance_on_PostgreSQL(PostgreSqlFixture postgreSql)
    {
        this._postgreSql = postgreSql;
    }

    public static TheoryData<InheritanceMapping> Mappings => [InheritanceMapping.Tph, InheritanceMapping.Tpt, InheritanceMapping.Tpc];

    [Theory(DisplayName = "CreateDb with [PrimaryKey] on a base class property in an inheritance hierarchy on PostgreSQL")]
    [MemberData(nameof(Mappings))]
    public void CreateDb_with_Inheritance_Test(InheritanceMapping mapping)
    {
        var connStr = this._postgreSql.CreateDatabase();
        using var db = InheritanceDbContext.Create(b => b.UseNpgsql(connStr), mapping);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            // Validate the primary keys and the indexes.
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"
                        SELECT indexdef FROM pg_indexes
                        WHERE schemaname = 'public'
                        ORDER BY tablename, indexname COLLATE ""C""";
            var dump = new List<string>();
            var r = cmd.ExecuteReader();
            try { while (r.Read()) dump.Add(r.GetString(0)); }
            finally { r.Close(); }

            switch (mapping)
            {
                case InheritanceMapping.Tph:
                    dump.Is(
                        @"CREATE INDEX ""IX_Items_Isbn"" ON public.""Items"" USING btree (""Isbn"")",
                        @"CREATE INDEX ""IX_Items_Name"" ON public.""Items"" USING btree (""Name"")",
                        @"CREATE UNIQUE INDEX ""PK_Items"" ON public.""Items"" USING btree (""Code"")");
                    break;
                case InheritanceMapping.Tpt:
                    dump.Is(
                        @"CREATE INDEX ""IX_Books_Isbn"" ON public.""Books"" USING btree (""Isbn"")",
                        @"CREATE UNIQUE INDEX ""PK_Books"" ON public.""Books"" USING btree (""Code"")",
                        @"CREATE UNIQUE INDEX ""PK_Gadgets"" ON public.""Gadgets"" USING btree (""Code"")",
                        @"CREATE INDEX ""IX_Items_Name"" ON public.""Items"" USING btree (""Name"")",
                        @"CREATE UNIQUE INDEX ""PK_Items"" ON public.""Items"" USING btree (""Code"")");
                    break;
                case InheritanceMapping.Tpc:
                    dump.Is(
                        @"CREATE INDEX ""IX_Books_Isbn"" ON public.""Books"" USING btree (""Isbn"")",
                        @"CREATE INDEX ""IX_Books_Name"" ON public.""Books"" USING btree (""Name"")",
                        @"CREATE UNIQUE INDEX ""PK_Books"" ON public.""Books"" USING btree (""Code"")",
                        @"CREATE INDEX ""IX_Gadgets_Name"" ON public.""Gadgets"" USING btree (""Name"")",
                        @"CREATE UNIQUE INDEX ""PK_Gadgets"" ON public.""Gadgets"" USING btree (""Code"")",
                        @"CREATE INDEX ""IX_Items_Name"" ON public.""Items"" USING btree (""Name"")",
                        @"CREATE UNIQUE INDEX ""PK_Items"" ON public.""Items"" USING btree (""Code"")");
                    break;
            }
        }
        finally
        {
            // PostgreSQL can't drop a database while a connection to it is open.
            db.Database.CloseConnection();
            db.Database.EnsureDeleted();
        }
    }
}
