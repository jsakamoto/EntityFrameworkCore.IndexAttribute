using EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest;

/// <summary>
/// Issue #12. [PrimaryKey] on a base class property must not make EF Core throw "A key cannot be configured on ... because it is a derived type".
/// </summary>
[Collection(nameof(SqlServerCollection))]
public class IndexAttributeTest_Inheritance
{
    private readonly SqlServerFixture _sqlServer;

    public IndexAttributeTest_Inheritance(SqlServerFixture sqlServer)
    {
        this._sqlServer = sqlServer;
    }

    public static TheoryData<InheritanceMapping> Mappings => [InheritanceMapping.Tph, InheritanceMapping.Tpt, InheritanceMapping.Tpc];

    [Theory(DisplayName = "CreateDb with [PrimaryKey] on a base class property in an inheritance hierarchy for SQL Server")]
    [MemberData(nameof(Mappings))]
    public void CreateDb_with_Inheritance_Test(InheritanceMapping mapping)
    {
        var connStr = this._sqlServer.CreateDatabase();
        using var db = InheritanceDbContext.Create(b => b.UseSqlServer(connStr), mapping, forSqlServer: true);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();

        try
        {
            // Validate the primary keys and the indexes. The primary keys must be NONCLUSTERED on every table.
            if (!(db.Database.GetDbConnection() is SqlConnection conn)) throw new NullReferenceException("db.Database.GetDbConnection() returns not SqlConnection.");
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                        SELECT [Table] = t.name, [Index] = ind.name, [Column] = col.name, [IsPrimaryKey] = ind.is_primary_key, [IsUnique] = ind.is_unique, [Type] = ind.type_desc
                        FROM sys.indexes ind
                        INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id
                        INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id
                        INNER JOIN sys.tables t ON ind.object_id = t.object_id
                        WHERE t.is_ms_shipped = 0
                        ORDER BY t.name, ind.name, ind.index_id, ic.key_ordinal;";
            var dump = new List<string>();
            var r = cmd.ExecuteReader();
            try { while (r.Read()) dump.Add($"{r["Table"]}|{r["Index"]}|{r["Column"]}|{r["IsPrimaryKey"]}|{r["IsUnique"]}|{r["Type"]}"); }
            finally { r.Close(); }

            switch (mapping)
            {
                case InheritanceMapping.Tph:
                    dump.Is(
                        "Items|IX_Items_Isbn|Isbn|False|False|NONCLUSTERED",
                        "Items|IX_Items_Name|Name|False|False|NONCLUSTERED",
                        "Items|PK_Items|Code|True|True|NONCLUSTERED");
                    break;
                case InheritanceMapping.Tpt:
                    dump.Is(
                        "Books|IX_Books_Isbn|Isbn|False|False|NONCLUSTERED",
                        "Books|PK_Books|Code|True|True|NONCLUSTERED",
                        "Gadgets|PK_Gadgets|Code|True|True|NONCLUSTERED",
                        "Items|IX_Items_Name|Name|False|False|NONCLUSTERED",
                        "Items|PK_Items|Code|True|True|NONCLUSTERED");
                    break;
                case InheritanceMapping.Tpc:
                    dump.Is(
                        "Books|IX_Books_Isbn|Isbn|False|False|NONCLUSTERED",
                        "Books|IX_Books_Name|Name|False|False|NONCLUSTERED",
                        "Books|PK_Books|Code|True|True|NONCLUSTERED",
                        "Gadgets|IX_Gadgets_Name|Name|False|False|NONCLUSTERED",
                        "Gadgets|PK_Gadgets|Code|True|True|NONCLUSTERED",
                        "Items|IX_Items_Name|Name|False|False|NONCLUSTERED",
                        "Items|PK_Items|Code|True|True|NONCLUSTERED");
                    break;
            }
        }
        finally { db.Database.EnsureDeleted(); }
    }
}
