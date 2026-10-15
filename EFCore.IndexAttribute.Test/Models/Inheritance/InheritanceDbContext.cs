using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Toolbelt.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.IndexAttributeTest.Models.Inheritance;

public class InheritanceDbContext : DbContext
{
    public DbSet<Item> Items { get; set; } = null!;

    public DbSet<Book> Books { get; set; } = null!;

    public DbSet<Gadget> Gadgets { get; set; } = null!;

    public InheritanceMapping Mapping { get; }

    public bool ForSqlServer { get; }

    public InheritanceDbContext(DbContextOptions options, InheritanceMapping mapping, bool forSqlServer) : base(options)
    {
        this.Mapping = mapping;
        this.ForSqlServer = forSqlServer;
    }

    /// <summary>
    /// Creates a context whose model cache is separated by the mapping and the SQL Server flag.
    /// </summary>
    public static InheritanceDbContext Create(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> useProvider, InheritanceMapping mapping, bool forSqlServer = false)
    {
        var builder = useProvider(new DbContextOptionsBuilder());
        builder.ReplaceService<IModelCacheKeyFactory, InheritanceModelCacheKeyFactory>();
        return new InheritanceDbContext(builder.Options, mapping, forSqlServer);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        switch (this.Mapping)
        {
            case InheritanceMapping.Tpt: modelBuilder.Entity<Item>().UseTptMappingStrategy(); break;
            case InheritanceMapping.Tpc: modelBuilder.Entity<Item>().UseTpcMappingStrategy(); break;
        }

        if (this.ForSqlServer) modelBuilder.BuildIndexesFromAnnotationsForSqlServer();
        else modelBuilder.BuildIndexesFromAnnotations();
    }
}

public class InheritanceModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
        => context is InheritanceDbContext c ? (c.GetType(), c.Mapping, c.ForSqlServer, designTime) : (context.GetType(), designTime);
}
