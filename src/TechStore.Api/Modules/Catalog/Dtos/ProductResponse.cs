namespace TechStore.Api.Modules.Catalog.Dtos;

public sealed record ProductResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    Guid CategoriaId,
    string? CategoriaNome,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? DataAtualizacao);
