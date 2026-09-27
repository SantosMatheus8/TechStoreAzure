using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Categories;
using TechStore.Api.Modules.Categories.Entities;
using TechStore.Api.Modules.Categories.Repositories;

namespace TechStore.Tests.Categories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly CategoriesDbContext _db;
    private readonly CategoryRepository _repo;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CategoriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new CategoriesDbContext(options);
        _repo = new CategoryRepository(_db);
    }

    [Fact]
    public async Task Deve_Criar_E_Retornar_Categoria()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Nome = "Teclados",
            Descricao = "Teclados mecânicos e de membrana"
        };

        var created = await _repo.CreateAsync(category);

        Assert.Equal(category.Id, created.Id);
        Assert.Equal("Teclados", created.Nome);
    }

    [Fact]
    public async Task Deve_Retornar_Categoria_Por_Id()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Mouses", Descricao = "Mouses gamer" });
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(id);

        Assert.NotNull(found);
        Assert.Equal("Mouses", found.Nome);
    }

    [Fact]
    public async Task Deve_Retornar_Null_Para_Id_Inexistente()
    {
        var found = await _repo.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task Deve_Paginar_Categorias()
    {
        for (int i = 1; i <= 15; i++)
            _db.Categories.Add(new Category { Id = Guid.NewGuid(), Nome = $"Cat {i:D2}", Descricao = "" });
        await _db.SaveChangesAsync();

        var page1 = await _repo.GetAllAsync(1, 10, null);
        var page2 = await _repo.GetAllAsync(2, 10, null);

        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(15, page1.TotalCount);
        Assert.True(page1.HasNext);
        Assert.False(page2.HasNext);
    }

    [Fact]
    public async Task Deve_Filtrar_Por_Nome()
    {
        _db.Categories.Add(new Category { Id = Guid.NewGuid(), Nome = "Notebooks", Descricao = "" });
        _db.Categories.Add(new Category { Id = Guid.NewGuid(), Nome = "Monitores", Descricao = "" });
        await _db.SaveChangesAsync();

        var result = await _repo.GetAllAsync(1, 10, "Note");

        Assert.Single(result.Items);
        Assert.Equal("Notebooks", result.Items[0].Nome);
    }

    [Fact]
    public async Task SoftDelete_Deve_Marcar_Como_Inativo()
    {
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Nome = "Temporária", Descricao = "" };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        category.Ativo = false;
        await _repo.UpdateAsync(category);

        var updated = await _repo.GetByIdAsync(id);
        Assert.NotNull(updated);
        Assert.False(updated.Ativo);
    }

    [Fact]
    public async Task Exists_Deve_Retornar_False_Para_Inativa()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Inativa", Descricao = "", Ativo = false });
        await _db.SaveChangesAsync();

        var exists = await _repo.ExistsAsync(id);
        Assert.False(exists);
    }

    public void Dispose() => _db.Dispose();
}
