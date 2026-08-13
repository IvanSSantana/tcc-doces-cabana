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
        services.AddScoped<ISubcategoriaRepository, SubcategoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAdministradorService, AdministradorService>();
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<ISubcategoriaService, SubcategoriaService>();
        services.AddEmailService(configuration);

        return services;
    }

    // Isolado do restante do registro para ser testável sem montar o grafo de
    // dependências inteiro (repositórios, UnitOfWork, DbContext) — ver
    // RegistroDeEmailTests (spec 007).
    public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        var adaptador = configuration["EmailSettings:Adaptador"];

        if (adaptador == "Arquivo")
            services.AddScoped<IEmailService, EmailServiceArquivo>();
        else
            services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}