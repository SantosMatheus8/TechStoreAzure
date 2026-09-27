using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechStore.Api.Modules.Categories;

public sealed class CategoriesDbContextFactory : IDesignTimeDbContextFactory<CategoriesDbContext>
{
    public CategoriesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CategoriesDbContext>()
            .UseSqlServer("Server=.;Database=TechStoreDb;Trusted_Connection=True;TrustServerCertificate=True;",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "categories"))
            .Options;

        return new CategoriesDbContext(options);
    }
}
