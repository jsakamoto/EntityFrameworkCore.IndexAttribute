using System;
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        public DbSet<SNSAccount> SNSAccounts { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().OwnsOne(p => p.Address);

            modelBuilder.BuildIndexesFromAnnotations();
        }
    }
}
