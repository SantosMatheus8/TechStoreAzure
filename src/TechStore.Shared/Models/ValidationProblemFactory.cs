using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace TechStore.Shared.Models;

public static class ValidationProblemFactory
{
    public static IResult CreateProblem(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors, title: "Erro de validação", statusCode: 400);
    }
}
