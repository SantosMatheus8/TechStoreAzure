using TechStore.Api.Modules.Categories.Entities;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Categories.Repositories;

public interface ICategoryRepository
{
    Task<PagedResult<Category>> GetAllAsync(int page, int pageSize, string? nome, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
