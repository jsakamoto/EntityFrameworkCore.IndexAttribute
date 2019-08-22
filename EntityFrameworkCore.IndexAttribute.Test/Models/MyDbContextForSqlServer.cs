using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class MyDbContextForSqlServer : MyDbContextBase
    {
        public MyDbContextForSqlServer(DbContextOptions<MyDbContextBase> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
        }
    }
}
