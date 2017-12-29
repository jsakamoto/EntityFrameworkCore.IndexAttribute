using System;
using System.Collections.Generic;
using EntityFrameworkCore.IndexAttributeTest.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest
{
    public class IndexAttributeTest_on_SQLite
    {
        [Fact(DisplayName = "CreateDb with Indexes on SQLite")]
        public void CreateDb_with_Indexes_Test()
        {
            var option = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;
            using (var db = new MyDbContext(option))
            {
                db.Database.OpenConnection();

                // Create database.
                db.Database.EnsureCreated();

                // Validate database indexes.
                var conn = db.Database.GetDbConnection();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT * FROM sqlite_master 
                        WHERE type = 'index'
                        ORDER BY tbl_name, name, sql";
                    var reader = cmd.ExecuteReader();
                    var dump = new List<string>();
                    try { while (reader.Read()) dump.Add(reader[4].ToString()); }
                    finally { reader.Close(); }
                    dump.Is(
                        @"CREATE UNIQUE INDEX ""IX_People_Name"" ON ""People"" (""Name"")",
                        @"CREATE INDEX ""IX_SNSAccounts_PersonId"" ON ""SNSAccounts"" (""PersonId"")",
                        @"CREATE INDEX ""IX_SNSAccounts_Provider"" ON ""SNSAccounts"" (""Provider"")",
                        @"CREATE UNIQUE INDEX ""Ix_Provider_and_Account"" ON ""SNSAccounts"" (""Provider"", ""AccountName"")");
                }
            }
        }
    }
}
