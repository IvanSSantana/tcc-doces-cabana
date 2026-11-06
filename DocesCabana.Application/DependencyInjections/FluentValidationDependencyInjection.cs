using DocesCabana.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Application.DependencyInjections;

public static class FluentValidationDependencyInjection
{
    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CadastroDTOValidator>();

        return services;
    }
}
