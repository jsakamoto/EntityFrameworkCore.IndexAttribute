using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models;

public class MyDbContext : MyDbContextBase
{
    public MyDbContext(DbContextOptions<MyDbContextBase> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.BuildIndexesFromAnnotations(options =>
        {
            options.SuppressNotSupportedException.IsClustered = true;
            options.SuppressNotSupportedException.Includes = true;
        });
    }
}
