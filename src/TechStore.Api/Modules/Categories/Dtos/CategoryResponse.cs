namespace TechStore.Api.Modules.Categories.Dtos;

public sealed record CategoryResponse(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo,
    DateTime DataCriacao);
