using TechStore.Api.Modules.Categories.Dtos;
using TechStore.Api.Modules.Categories.Validators;

namespace TechStore.Tests.Categories;

public class CategoryValidatorTests
{
    private readonly CategoryRequestValidator _validator = new();

    [Fact]
    public async Task Deve_Validar_Categoria_Valida()
    {
        var request = new CategoryRequest("Notebooks", "Notebooks e laptops");
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Deve_Rejeitar_Nome_Vazio()
    {
        var request = new CategoryRequest("", "Descrição");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task Deve_Rejeitar_Nome_Acima_De_100_Caracteres()
    {
        var request = new CategoryRequest(new string('A', 101), "Descrição");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task Deve_Rejeitar_Descricao_Acima_De_500_Caracteres()
    {
        var request = new CategoryRequest("Válida", new string('A', 501));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Descricao");
    }
}
