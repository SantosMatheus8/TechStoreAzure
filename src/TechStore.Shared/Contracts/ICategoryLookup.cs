namespace TechStore.Shared.Contracts;

public interface ICategoryLookup
{
    Task<bool> ExistsAsync(Guid categoryId, CancellationToken ct = default);
    Task<string?> GetNameAsync(Guid categoryId, CancellationToken ct = default);
}
