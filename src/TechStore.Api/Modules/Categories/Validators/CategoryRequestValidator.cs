using FluentValidation;
using TechStore.Api.Modules.Categories.Dtos;

namespace TechStore.Api.Modules.Categories.Validators;

public sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório.")
            .MaximumLength(100).WithMessage("O nome da categoria deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
    }
}
