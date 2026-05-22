using DocesCabana.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Application.DependencyInjections;

public static class FluentValidationDependencyInjection
{
    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<CadastroDTOValidator>(); // Nome de qualquer classe somente para o Assembly encontrar os validators.

        return services;
    }
}
