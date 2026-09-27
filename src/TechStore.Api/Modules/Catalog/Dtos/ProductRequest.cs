namespace TechStore.Api.Modules.Catalog.Dtos;

public sealed record ProductRequest(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    Guid CategoriaId);
