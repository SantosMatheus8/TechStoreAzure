using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Categories.Endpoints;
using TechStore.Api.Modules.Categories.Repositories;
using TechStore.Api.Modules.Categories.Services;
using TechStore.Shared.Contracts;

namespace TechStore.Api.Modules.Categories;

public static class CategoriesModule
{
    public static IServiceCollection AddCategoriesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CategoriesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "categories")));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryLookup, CategoryLookupService>();

        return services;
    }

    public static WebApplication MapCategoriesEndpoints(this WebApplication app)
    {
        app.MapCategoryEndpoints();
        return app;
    }
}
