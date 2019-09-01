using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class MyDbContextWithNoConfiguration : MyDbContextBase
    {
        public MyDbContextWithNoConfiguration(DbContextOptions<MyDbContextBase> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.BuildIndexesFromAnnotations(/* WITH NO CONFIGURATION */);
        }
    }
}
