using TechStore.Api.Modules.Catalog.Dtos;
using TechStore.Api.Modules.Catalog.Validators;

namespace TechStore.Tests.Catalog;

public class ProductValidatorTests
{
    private readonly ProductRequestValidator _validator = new();

    [Fact]
    public async Task Deve_Validar_Produto_Valido()
    {
        var request = new ProductRequest("Notebook", "Bom notebook", 2999.99m, 10, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Deve_Rejeitar_Nome_Vazio()
    {
        var request = new ProductRequest("", "Descrição", 100m, 1, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task Deve_Rejeitar_Nome_Acima_De_200_Caracteres()
    {
        var request = new ProductRequest(new string('A', 201), "Descrição", 100m, 1, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Deve_Rejeitar_Preco_Zero()
    {
        var request = new ProductRequest("Produto", "Desc", 0m, 1, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public async Task Deve_Rejeitar_Preco_Negativo()
    {
        var request = new ProductRequest("Produto", "Desc", -10m, 1, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Preco");
    }

    [Fact]
    public async Task Deve_Rejeitar_Estoque_Negativo()
    {
        var request = new ProductRequest("Produto", "Desc", 100m, -1, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Estoque");
    }

    [Fact]
    public async Task Deve_Aceitar_Estoque_Zero()
    {
        var request = new ProductRequest("Produto", "Desc", 100m, 0, Guid.NewGuid());
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Deve_Rejeitar_CategoriaId_Vazio()
    {
        var request = new ProductRequest("Produto", "Desc", 100m, 1, Guid.Empty);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CategoriaId");
    }
}
