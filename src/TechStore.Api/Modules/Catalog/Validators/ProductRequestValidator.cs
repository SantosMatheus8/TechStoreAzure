using FluentValidation;
using TechStore.Api.Modules.Catalog.Dtos;

namespace TechStore.Api.Modules.Catalog.Validators;

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do produto é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do produto deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

        RuleFor(x => x.Preco)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0).WithMessage("O estoque não pode ser negativo.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("A categoria é obrigatória.");
    }
}
