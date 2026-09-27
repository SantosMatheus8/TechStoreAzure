using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Catalog.Endpoints;
using TechStore.Api.Modules.Catalog.Repositories;

namespace TechStore.Api.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog")));

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }

    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        app.MapProductEndpoints();
        return app;
    }
}
