﻿using System;
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
            var option = new DbContextOptionsBuilder<MyDbContextBase>()
                .UseSqlite("Data Source=:memory:")
                .Options;
            using var db = new MyDbContext(option);
            db.Database.OpenConnection();

            // Create database.
            db.Database.EnsureCreated();

            // Validate database indexes.
            var conn = db.Database.GetDbConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                        SELECT * FROM sqlite_master 
                        WHERE type = 'index'
                        ORDER BY tbl_name, name, sql";
            var reader = cmd.ExecuteReader();
            var dump = new List<string>();
            try { while (reader.Read()) dump.Add(reader[4]?.ToString() ?? throw new NullReferenceException("reader[4] returns null.")); }
            finally { reader.Close(); }
            dump.Is(
                @"CREATE INDEX ""IX_Country"" ON ""People"" (""Address_Country"")",
                @"CREATE INDEX ""IX_Lines"" ON ""People"" (""Line1"", ""Line2"")",
                @"CREATE INDEX ""IX_People_FaxNumber_CountryCode"" ON ""People"" (""FaxNumber_CountryCode"")",
                @"CREATE UNIQUE INDEX ""IX_People_Name"" ON ""People"" (""Name"")",
                @"CREATE INDEX ""IX_People_PhoneNumber_CountryCode"" ON ""People"" (""PhoneNumber_CountryCode"")",
                @"CREATE INDEX ""IX_SNSAccounts_PersonId"" ON ""SNSAccounts"" (""PersonId"")",
                @"CREATE INDEX ""IX_SNSAccounts_Provider"" ON ""SNSAccounts"" (""Provider"")",
                @"CREATE UNIQUE INDEX ""Ix_Provider_and_Account"" ON ""SNSAccounts"" (""Provider"", ""AccountName"")");
        }

        [Fact(DisplayName = "IsClustered=true with no configuration causes NotSupportedException")]
        public void IsCLusteredTrue_With_NoConfiguration_Causes_NotSupportedException_Test()
        {
            var option = new DbContextOptionsBuilder<MyDbContextBase>()
                .UseSqlite("Data Source=:memory:")
                .Options;
            using var db = new MyDbContextWithNoConfiguration(option);
            var e = Assert.ThrowsAny<AggregateException>(() =>
            {
                db.Database.OpenConnection();
            });
            e.InnerException.IsInstanceOf<NotSupportedException>();
        }
    }
}
