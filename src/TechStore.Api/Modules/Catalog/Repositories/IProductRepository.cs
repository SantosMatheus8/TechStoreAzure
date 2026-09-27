using TechStore.Api.Modules.Catalog.Entities;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Catalog.Repositories;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetAllAsync(int page, int pageSize, string? nome, Guid? categoriaId, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
}
