using Microsoft.EntityFrameworkCore;
using TestProject.Models;
using Toolbelt.ComponentModel.DataAnnotations;

namespace TestProject.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // modelBuilder.BuildIndexesFromAnnotations();
            modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
        }
    }
}
