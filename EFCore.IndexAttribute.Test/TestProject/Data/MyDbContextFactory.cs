using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TestProject.Data;

public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            //.UseSqlite("Data Source=:memory:")
            .UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;Initial Catalog=test.mdf;")
            .Options;
        var dbContext = new MyDbContext(options);
        return dbContext;
    }
}
