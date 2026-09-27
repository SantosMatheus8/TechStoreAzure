using Microsoft.EntityFrameworkCore;
using TechStore.Shared.Contracts;

namespace TechStore.Api.Modules.Categories.Services;

public sealed class CategoryLookupService : ICategoryLookup
{
    private readonly CategoriesDbContext _db;

    public CategoryLookupService(CategoriesDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(c => c.Id == categoryId && c.Ativo, ct);

    public async Task<string?> GetNameAsync(Guid categoryId, CancellationToken ct = default)
        => await _db.Categories
            .Where(c => c.Id == categoryId && c.Ativo)
            .Select(c => c.Nome)
            .FirstOrDefaultAsync(ct);
}
