using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.Identity.Services;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocesCabana.Infrastructure.DependencyInjections;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServicesAndRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}