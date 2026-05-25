using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.Identity.Services;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Infrastructure.DependencyInjections;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServicesAndRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IUsuarioServices, UsuarioServices>();
        services.AddScoped<IProdutoServices, ProdutoServices>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}