using DocesCabana.Application.Contracts.Repositories;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.Services;
using DocesCabana.Domain.Contracts;
using DocesCabana.Infrastructure.Identity.Services;
using DocesCabana.Infrastructure.Repositories;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DocesCabana.Infrastructure.DependencyInjections;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServicesAndRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<FreteSettings>(configuration.GetSection("FreteSettings"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<ISubcategoriaRepository, SubcategoriaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
        services.AddScoped<IFavoritoRepository, FavoritoRepository>();
        services.AddScoped<IItemCarrinhoRepository, ItemCarrinhoRepository>();
        services.AddScoped<IEnderecoRepository, EnderecoRepository>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IAdministradorService, AdministradorService>();
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<ISubcategoriaService, SubcategoriaService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<ICarrinhoService, CarrinhoService>();
        services.AddScoped<IEnderecoService, EnderecoService>();
        services.AddEmailService(configuration);
        services.AddFreteService();

        return services;
    }

    // Isolado pelo mesmo motivo de AddEmailService: testável sem montar o
    // grafo inteiro. Uma implementação só (spec 020 §10) — sem simulador ao
    // lado, então sem o "if" por adaptador que AddEmailService tem.
    public static IServiceCollection AddFreteService(this IServiceCollection services)
    {
        services.AddHttpClient<IFreteService, FreteServiceMelhorEnvio>((provedor, client) =>
        {
            var settings = provedor.GetRequiredService<IOptions<FreteSettings>>().Value;
            client.BaseAddress = new Uri(settings.UrlBase);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutEmSegundos);
        });

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