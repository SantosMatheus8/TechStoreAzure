using TechStore.Api.Modules.Catalog.Dtos;
using TechStore.Api.Modules.Catalog.Entities;
using TechStore.Api.Modules.Catalog.Repositories;
using TechStore.Api.Modules.Catalog.Validators;
using TechStore.Shared.Contracts;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Catalog.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/produtos").WithTags("Produtos");

        group.MapGet("/", GetAll).WithName("GetProdutos");
        group.MapGet("/{id:guid}", GetById).WithName("GetProdutoById");
        group.MapPost("/", Create).WithName("CreateProduto");
        group.MapPut("/{id:guid}", Update).WithName("UpdateProduto");
        group.MapDelete("/{id:guid}", Delete).WithName("DeleteProduto");

        return group;
    }

    private static async Task<IResult> GetAll(
        IProductRepository repo,
        ICategoryLookup categoryLookup,
        string? nome = null,
        Guid? categoriaId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var result = await repo.GetAllAsync(page, pageSize, nome, categoriaId, ct);

        var items = new List<ProductResponse>();
        foreach (var p in result.Items)
        {
            var categoriaNome = await categoryLookup.GetNameAsync(p.CategoriaId, ct);
            items.Add(MapToResponse(p, categoriaNome));
        }

        var response = new PagedResult<ProductResponse>
        {
            Items = items,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IProductRepository repo,
        ICategoryLookup categoryLookup,
        CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null)
            return Results.NotFound();

        var categoriaNome = await categoryLookup.GetNameAsync(product.CategoriaId, ct);
        return Results.Ok(MapToResponse(product, categoriaNome));
    }

    private static async Task<IResult> Create(
        ProductRequest request,
        IProductRepository repo,
        ICategoryLookup categoryLookup,
        CancellationToken ct)
    {
        var validator = new ProductRequestValidator();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblemFactory.CreateProblem(validation);

        var categoryExists = await categoryLookup.ExistsAsync(request.CategoriaId, ct);
        if (!categoryExists)
            return Results.Problem(
                title: "Categoria não encontrada",
                detail: $"A categoria com ID '{request.CategoriaId}' não existe ou está inativa.",
                statusCode: 422);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            Estoque = request.Estoque,
            CategoriaId = request.CategoriaId
        };

        await repo.CreateAsync(product, ct);

        var categoriaNome = await categoryLookup.GetNameAsync(product.CategoriaId, ct);
        return Results.Created($"/api/produtos/{product.Id}", MapToResponse(product, categoriaNome));
    }

    private static async Task<IResult> Update(
        Guid id,
        ProductRequest request,
        IProductRepository repo,
        ICategoryLookup categoryLookup,
        CancellationToken ct)
    {
        var validator = new ProductRequestValidator();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblemFactory.CreateProblem(validation);

        var product = await repo.GetByIdAsync(id, ct);
        if (product is null)
            return Results.NotFound();

        var categoryExists = await categoryLookup.ExistsAsync(request.CategoriaId, ct);
        if (!categoryExists)
            return Results.Problem(
                title: "Categoria não encontrada",
                detail: $"A categoria com ID '{request.CategoriaId}' não existe ou está inativa.",
                statusCode: 422);

        product.Nome = request.Nome;
        product.Descricao = request.Descricao;
        product.Preco = request.Preco;
        product.Estoque = request.Estoque;
        product.CategoriaId = request.CategoriaId;
        product.DataAtualizacao = DateTime.UtcNow;

        await repo.UpdateAsync(product, ct);

        var categoriaNome = await categoryLookup.GetNameAsync(product.CategoriaId, ct);
        return Results.Ok(MapToResponse(product, categoriaNome));
    }

    private static async Task<IResult> Delete(
        Guid id,
        IProductRepository repo,
        CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null)
            return Results.NotFound();

        product.Ativo = false;
        product.DataAtualizacao = DateTime.UtcNow;
        await repo.UpdateAsync(product, ct);
        return Results.NoContent();
    }

    private static ProductResponse MapToResponse(Product p, string? categoriaNome) =>
        new(p.Id, p.Nome, p.Descricao, p.Preco, p.Estoque,
            p.CategoriaId, categoriaNome, p.Ativo, p.DataCriacao, p.DataAtualizacao);
}
