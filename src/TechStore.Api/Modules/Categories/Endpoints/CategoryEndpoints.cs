using TechStore.Api.Modules.Categories.Dtos;
using TechStore.Api.Modules.Categories.Entities;
using TechStore.Api.Modules.Categories.Repositories;
using TechStore.Api.Modules.Categories.Validators;
using TechStore.Shared.Models;

namespace TechStore.Api.Modules.Categories.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categorias").WithTags("Categorias");

        group.MapGet("/", GetAll).WithName("GetCategorias");
        group.MapGet("/{id:guid}", GetById).WithName("GetCategoriaById");
        group.MapPost("/", Create).WithName("CreateCategoria");
        group.MapPut("/{id:guid}", Update).WithName("UpdateCategoria");
        group.MapDelete("/{id:guid}", Delete).WithName("DeleteCategoria");

        return group;
    }

    private static async Task<IResult> GetAll(
        ICategoryRepository repo,
        string? nome = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var result = await repo.GetAllAsync(page, pageSize, nome, ct);

        var response = new PagedResult<CategoryResponse>
        {
            Items = result.Items.Select(MapToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> GetById(Guid id, ICategoryRepository repo, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);
        return category is null
            ? Results.NotFound()
            : Results.Ok(MapToResponse(category));
    }

    private static async Task<IResult> Create(
        CategoryRequest request,
        ICategoryRepository repo,
        CancellationToken ct)
    {
        var validator = new CategoryRequestValidator();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblemFactory.CreateProblem(validation);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao
        };

        await repo.CreateAsync(category, ct);
        return Results.Created($"/api/categorias/{category.Id}", MapToResponse(category));
    }

    private static async Task<IResult> Update(
        Guid id,
        CategoryRequest request,
        ICategoryRepository repo,
        CancellationToken ct)
    {
        var validator = new CategoryRequestValidator();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblemFactory.CreateProblem(validation);

        var category = await repo.GetByIdAsync(id, ct);
        if (category is null)
            return Results.NotFound();

        category.Nome = request.Nome;
        category.Descricao = request.Descricao;

        await repo.UpdateAsync(category, ct);
        return Results.Ok(MapToResponse(category));
    }

    private static async Task<IResult> Delete(Guid id, ICategoryRepository repo, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);
        if (category is null)
            return Results.NotFound();

        category.Ativo = false;
        await repo.UpdateAsync(category, ct);
        return Results.NoContent();
    }

    private static CategoryResponse MapToResponse(Category c)
        => new(c.Id, c.Nome, c.Descricao, c.Ativo, c.DataCriacao);
}
