using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Categories;
using TechStore.Api.Modules.Categories.Entities;
using TechStore.Api.Modules.Categories.Services;

namespace TechStore.Tests.Catalog;

public class CategoryLookupTests : IDisposable
{
    private readonly CategoriesDbContext _db;
    private readonly CategoryLookupService _lookup;

    public CategoryLookupTests()
    {
        var options = new DbContextOptionsBuilder<CategoriesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new CategoriesDbContext(options);
        _lookup = new CategoryLookupService(_db);
    }

    [Fact]
    public async Task Exists_Deve_Retornar_True_Para_Categoria_Ativa()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Ativa", Descricao = "", Ativo = true });
        await _db.SaveChangesAsync();

        Assert.True(await _lookup.ExistsAsync(id));
    }

    [Fact]
    public async Task Exists_Deve_Retornar_False_Para_Categoria_Inativa()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Inativa", Descricao = "", Ativo = false });
        await _db.SaveChangesAsync();

        Assert.False(await _lookup.ExistsAsync(id));
    }

    [Fact]
    public async Task Exists_Deve_Retornar_False_Para_Id_Inexistente()
    {
        Assert.False(await _lookup.ExistsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetName_Deve_Retornar_Nome_Da_Categoria_Ativa()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Notebooks", Descricao = "", Ativo = true });
        await _db.SaveChangesAsync();

        var name = await _lookup.GetNameAsync(id);
        Assert.Equal("Notebooks", name);
    }

    [Fact]
    public async Task GetName_Deve_Retornar_Null_Para_Categoria_Inativa()
    {
        var id = Guid.NewGuid();
        _db.Categories.Add(new Category { Id = id, Nome = "Inativa", Descricao = "", Ativo = false });
        await _db.SaveChangesAsync();

        var name = await _lookup.GetNameAsync(id);
        Assert.Null(name);
    }

    public void Dispose() => _db.Dispose();
}
