using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models.CompositeOptions;

public class CompositeOptionsDbContext : DbContext
{
    public DbSet<Visit> Visits { get; set; } = null!;

    public DbSet<Article> Articles { get; set; } = null!;

    public DbSet<OrderLine> OrderLines { get; set; } = null!;

    public CompositeOptionsDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        this.BuildIndexes(modelBuilder);
    }

    protected virtual void BuildIndexes(ModelBuilder modelBuilder) => modelBuilder.BuildIndexesFromAnnotations(options =>
    {
        options.SuppressNotSupportedException.IsClustered = true;
        options.SuppressNotSupportedException.Includes = true;
    });
}

public class CompositeOptionsDbContextForSqlServer : CompositeOptionsDbContext
{
    public CompositeOptionsDbContextForSqlServer(DbContextOptions options) : base(options)
    {
    }

    protected override void BuildIndexes(ModelBuilder modelBuilder) => modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
}
