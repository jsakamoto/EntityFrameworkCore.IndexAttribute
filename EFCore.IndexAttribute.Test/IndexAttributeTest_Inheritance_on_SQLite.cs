using EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

/// <summary>
/// Issue #12. [PrimaryKey] on a base class property must not make EF Core throw "A key cannot be configured on ... because it is a derived type".
/// </summary>
public class IndexAttributeTest_Inheritance_on_SQLite
{
    public static TheoryData<InheritanceMapping> Mappings => [InheritanceMapping.Tph, InheritanceMapping.Tpt, InheritanceMapping.Tpc];

    private static List<string> Query(DbContext db, string sql)
    {
        using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = sql;
        var result = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(reader.GetString(0));
        return result;
    }

    [Theory(DisplayName = "CreateDb with [PrimaryKey] on a base class property in an inheritance hierarchy on SQLite")]
    [MemberData(nameof(Mappings))]
    public void CreateDb_with_Inheritance_Test(InheritanceMapping mapping)
    {
        using var db = InheritanceDbContext.Create(b => b.UseSqlite("Data Source=:memory:"), mapping);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        // Validate the primary keys (table, column, and constraint name).
        var primaryKeys = Query(db, @"
            SELECT m.name || '|' || p.name || '|' || CASE WHEN m.sql LIKE '%CONSTRAINT ""PK_' || m.name || '"" PRIMARY KEY%' THEN 'PK_' || m.name ELSE '?' END
            FROM sqlite_master m, pragma_table_info(m.name) p
            WHERE m.type = 'table' AND p.pk > 0
            ORDER BY m.name, p.pk");

        // Validate the indexes.
        var indexes = Query(db, "SELECT sql FROM sqlite_master WHERE type = 'index' AND sql IS NOT NULL ORDER BY tbl_name, name");

        switch (mapping)
        {
            case InheritanceMapping.Tph:
                primaryKeys.Is("Items|Code|PK_Items");
                indexes.Is(
                    @"CREATE INDEX ""IX_Items_Isbn"" ON ""Items"" (""Isbn"")",
                    @"CREATE INDEX ""IX_Items_Name"" ON ""Items"" (""Name"")");
                break;
            case InheritanceMapping.Tpt:
                primaryKeys.Is("Books|Code|PK_Books", "Gadgets|Code|PK_Gadgets", "Items|Code|PK_Items");
                indexes.Is(
                    @"CREATE INDEX ""IX_Books_Isbn"" ON ""Books"" (""Isbn"")",
                    @"CREATE INDEX ""IX_Items_Name"" ON ""Items"" (""Name"")");
                break;
            case InheritanceMapping.Tpc:
                primaryKeys.Is("Books|Code|PK_Books", "Gadgets|Code|PK_Gadgets", "Items|Code|PK_Items");
                indexes.Is(
                    @"CREATE INDEX ""IX_Books_Isbn"" ON ""Books"" (""Isbn"")",
                    @"CREATE INDEX ""IX_Books_Name"" ON ""Books"" (""Name"")",
                    @"CREATE INDEX ""IX_Gadgets_Name"" ON ""Gadgets"" (""Name"")",
                    @"CREATE INDEX ""IX_Items_Name"" ON ""Items"" (""Name"")");
                break;
        }
    }
}
