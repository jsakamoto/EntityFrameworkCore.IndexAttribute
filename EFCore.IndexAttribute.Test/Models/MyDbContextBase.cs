using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class MyDbContextBase : DbContext
    {
        public DbSet<Person> People { get; set; }

        public DbSet<SNSAccount> SNSAccounts { get; set; }

#nullable disable
        public MyDbContextBase(DbContextOptions<MyDbContextBase> options) : base(options)
        {
        }
#nullable restore

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var person = modelBuilder.Entity<Person>();
            person.OwnsOne(p => p.Address, address =>
            {
                address.OwnsOne(a => a.Lines,
                    lines =>
                    {
                        lines.Property(l => l.Line1).HasColumnName("Line1");
                        lines.Property(l => l.Line2).HasColumnName("Line2");
                        lines.Property(l => l.Line3).HasColumnName("Line3");
                    });
            });
            person.OwnsOne(p => p.PhoneNumber);
            person.OwnsOne(p => p.FaxNumber);

            //modelBuilder.Entity<Person>().OwnsOne(typeof(Address), "Address", address =>
            //{
            //    address.OwnsOne(typeof(Lines), "Lines", l => l.HasIndex("Line1", "Line2"));
            //});
            //modelBuilder.Entity<Person>().HasIndex("Name").IsUnique();
            //modelBuilder.Entity<SNSAccount>().HasIndex("Provider");
            //modelBuilder.Entity<SNSAccount>().HasIndex("Provider", "AccountName").HasName("Ix_Provider_and_Account").IsUnique();
        }
    }
}
