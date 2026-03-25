using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LocalChat.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=App_Data/localchat.db",
            sqlite =>
            {
                sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
