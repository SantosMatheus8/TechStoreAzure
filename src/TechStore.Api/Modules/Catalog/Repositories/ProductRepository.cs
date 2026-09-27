using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Catalog.Entities;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Catalog.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext db) => _db = db;

    public async Task<PagedResult<Product>> GetAllAsync(int page, int pageSize, string? nome, Guid? categoriaId, CancellationToken ct = default)
    {
        var query = _db.Products.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => p.Nome.Contains(nome));

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.Ativo, ct);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }
}
