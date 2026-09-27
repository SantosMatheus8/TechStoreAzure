using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Categories.Entities;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Categories.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly CategoriesDbContext _db;

    public CategoryRepository(CategoriesDbContext db) => _db = db;

    public async Task<PagedResult<Category>> GetAllAsync(int page, int pageSize, string? nome, CancellationToken ct = default)
    {
        var query = _db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(c => c.Nome.Contains(nome));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Category>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.FindAsync([id], ct);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _db.Categories.AnyAsync(c => c.Id == id && c.Ativo, ct);
}
