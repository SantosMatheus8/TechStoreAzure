using Microsoft.EntityFrameworkCore;
using TechStore.Api.Modules.Catalog;
using TechStore.Api.Modules.Catalog.Entities;
using TechStore.Api.Modules.Catalog.Repositories;

namespace TechStore.Tests.Catalog;

public class ProductRepositoryTests : IDisposable
{
    private readonly CatalogDbContext _db;
    private readonly ProductRepository _repo;
    private readonly Guid _categoriaId = Guid.NewGuid();

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new CatalogDbContext(options);
        _repo = new ProductRepository(_db);
    }

    [Fact]
    public async Task Deve_Criar_E_Retornar_Produto()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nome = "Teclado Gamer",
            Descricao = "RGB e mecânico",
            Preco = 250m,
            Estoque = 10,
            CategoriaId = _categoriaId
        };

        var created = await _repo.CreateAsync(product);

        Assert.Equal(product.Id, created.Id);
        Assert.Equal("Teclado Gamer", created.Nome);
    }

    [Fact]
    public async Task Deve_Retornar_Produto_Ativo_Por_Id()
    {
        var id = Guid.NewGuid();
        _db.Products.Add(new Product
        {
            Id = id, Nome = "Mouse", Descricao = "", Preco = 100m,
            Estoque = 5, CategoriaId = _categoriaId, Ativo = true
        });
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(id);
        Assert.NotNull(found);
    }

    [Fact]
    public async Task Deve_Retornar_Null_Para_Produto_Inativo()
    {
        var id = Guid.NewGuid();
        _db.Products.Add(new Product
        {
            Id = id, Nome = "Excluído", Descricao = "", Preco = 100m,
            Estoque = 5, CategoriaId = _categoriaId, Ativo = false
        });
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(id);
        Assert.Null(found);
    }

    [Fact]
    public async Task Deve_Paginar_Produtos()
    {
        for (int i = 1; i <= 25; i++)
            _db.Products.Add(new Product
            {
                Id = Guid.NewGuid(), Nome = $"Produto {i:D2}", Descricao = "",
                Preco = 10m * i, Estoque = i, CategoriaId = _categoriaId
            });
        await _db.SaveChangesAsync();

        var page1 = await _repo.GetAllAsync(1, 10, null, null);
        var page3 = await _repo.GetAllAsync(3, 10, null, null);

        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(5, page3.Items.Count);
        Assert.Equal(25, page1.TotalCount);
    }

    [Fact]
    public async Task Deve_Filtrar_Por_Nome()
    {
        _db.Products.Add(new Product { Id = Guid.NewGuid(), Nome = "Notebook Dell", Descricao = "", Preco = 3000m, Estoque = 1, CategoriaId = _categoriaId });
        _db.Products.Add(new Product { Id = Guid.NewGuid(), Nome = "Mouse Logitech", Descricao = "", Preco = 200m, Estoque = 1, CategoriaId = _categoriaId });
        await _db.SaveChangesAsync();

        var result = await _repo.GetAllAsync(1, 10, "Notebook", null);

        Assert.Single(result.Items);
        Assert.Equal("Notebook Dell", result.Items[0].Nome);
    }

    [Fact]
    public async Task Deve_Filtrar_Por_CategoriaId()
    {
        var catA = Guid.NewGuid();
        var catB = Guid.NewGuid();
        _db.Products.Add(new Product { Id = Guid.NewGuid(), Nome = "Prod A", Descricao = "", Preco = 10m, Estoque = 1, CategoriaId = catA });
        _db.Products.Add(new Product { Id = Guid.NewGuid(), Nome = "Prod B", Descricao = "", Preco = 20m, Estoque = 1, CategoriaId = catB });
        await _db.SaveChangesAsync();

        var result = await _repo.GetAllAsync(1, 10, null, catA);

        Assert.Single(result.Items);
        Assert.Equal("Prod A", result.Items[0].Nome);
    }

    [Fact]
    public async Task SoftDelete_Deve_Ocultar_Produto_Do_GetAll()
    {
        var id = Guid.NewGuid();
        var product = new Product
        {
            Id = id, Nome = "Deletável", Descricao = "", Preco = 50m,
            Estoque = 1, CategoriaId = _categoriaId
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        product.Ativo = false;
        await _repo.UpdateAsync(product);

        var result = await _repo.GetAllAsync(1, 10, null, null);
        Assert.DoesNotContain(result.Items, p => p.Id == id);
    }

    public void Dispose() => _db.Dispose();
}
