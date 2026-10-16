using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models.Descending;

public class DescendingDbContext : DbContext
{
    public DbSet<Ranking> Rankings { get; set; } = null!;

    public DescendingDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.BuildIndexes(modelBuilder);
    }

    protected virtual void BuildIndexes(ModelBuilder modelBuilder) => modelBuilder.BuildIndexesFromAnnotations();
}

public class DescendingDbContextForSqlServer : DescendingDbContext
{
    public DescendingDbContextForSqlServer(DbContextOptions options) : base(options)
    {
    }

    protected override void BuildIndexes(ModelBuilder modelBuilder) => modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
}
