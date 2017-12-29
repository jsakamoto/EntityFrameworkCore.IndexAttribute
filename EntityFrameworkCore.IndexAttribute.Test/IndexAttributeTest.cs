using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using EntityFrameworkCore.IndexAttributeTest.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkCore.IndexAttributeTest
{
    public class IndexAttributeTest : IDisposable
    {
        public string DbName { get; }

        public IndexAttributeTest()
        {
            DbName = Guid.NewGuid().ToString("N");
            Helper.ExecuteQueryToMaster($"CREATE DATABASE [{DbName}]");
        }

        [Fact(DisplayName = "CreateDb with Indexes on MSSQL LocalDb")]
        public void CreateDb_with_Indexes_Test()
        {
            var connStr = $"Server=(localdb)\\mssqllocaldb;Database={DbName};Trusted_Connection=True;MultipleActiveResultSets=True;";

            // Create database.
            var option = new DbContextOptionsBuilder<MyDbContext>().UseSqlServer(connStr).Options;
            using (var db = new MyDbContext(option))
            {
                db.Database.EnsureCreated();
            }

            // Validate database indexes.
            using (var conn = new SqlConnection(connStr))
            using (var cmd = conn.CreateCommand())
            using (var da = new SqlDataAdapter(cmd))
            using (var table = new DataTable())
            {
                cmd.CommandText = @"
                    SELECT [Table] = t.name, [Index] = ind.name, [Column] = col.name, IsUnique = ind.is_unique
                    FROM sys.indexes ind 
                    INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
                    INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
                    INNER JOIN sys.tables t ON ind.object_id = t.object_id 
                    WHERE ind.is_primary_key = 0 AND t.is_ms_shipped = 0 
                    ORDER BY t.name, ind.name, ind.index_id, ic.key_ordinal;";
                conn.Open();
                da.Fill(table);
                var dump = table.Rows.Cast<DataRow>()
                    .Select(row => $"{row["Table"]}|{row["Index"]}|{row["Column"]}|{row["IsUnique"]}")
                    .ToArray();
                dump.Is(
                    "People|IX_People_Name|Name|True",
                    "SNSAccounts|Ix_Provider_and_Account|Provider|True",
                    "SNSAccounts|Ix_Provider_and_Account|AccountName|True",
                    "SNSAccounts|IX_SNSAccounts_PersonId|PersonId|False",
                    "SNSAccounts|IX_SNSAccounts_Provider|Provider|False"
                );
            }
        }

        public void Dispose()
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dataPhysicalPath = Path.Combine(baseDir, $"{DbName}.mdf");
            var logPhysicalPath = Path.Combine(baseDir, $"{DbName}_log.ldf");

            if (File.Exists(dataPhysicalPath) || File.Exists(logPhysicalPath))
            {
                Helper.ExecuteQueryToMaster($@"
                    ALTER database [{DbName}] set offline with ROLLBACK IMMEDIATE;
                    DROP DATABASE [{DbName}]");
            }
            if (File.Exists(dataPhysicalPath)) File.Delete(dataPhysicalPath);
            if (File.Exists(logPhysicalPath)) File.Delete(logPhysicalPath);
        }

        private static class Helper
        {
            internal static void ExecuteQueryToMaster(string sql)
            {
                const string connStrToMaster = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;MultipleActiveResultSets=True;";
                using (var conn = new SqlConnection(connStrToMaster))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
