using DocesCabana.Application.Contracts.Services;
using DocesCabana.Infrastructure.DependencyInjections;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace DocesCabana.Tests.Units.DependencyInjections;

public class RegistroDeEmailTests
{
    private static IEmailService ResolverEmailService(string? adaptador)
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:Adaptador"] = adaptador,
                ["EmailSettings:PastaDeSaida"] = "pasta-qualquer"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<EmailSettings>(configuracao.GetSection("EmailSettings"));
        services.AddEmailService(configuracao);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IEmailService>();
    }

    [Fact]
    public void Dado_AdaptadorArquivo_Quando_AddEmailService_Entao_DeveResolverEmailServiceArquivo()
    {
        var emailService = ResolverEmailService("Arquivo");

        Assert.IsType<EmailServiceArquivo>(emailService);
    }

    [Fact]
    public void Dado_AdaptadorAusente_Quando_AddEmailService_Entao_DeveResolverEmailService()
    {
        var emailService = ResolverEmailService(null);

        Assert.IsType<EmailService>(emailService);
    }

    [Fact]
    public void Dado_AdaptadorVazio_Quando_AddEmailService_Entao_DeveResolverEmailService()
    {
        var emailService = ResolverEmailService(string.Empty);

        Assert.IsType<EmailService>(emailService);
    }

    [Fact]
    public void Dado_AdaptadorDesconhecido_Quando_AddEmailService_Entao_DeveResolverEmailService()
    {
        // O padrão nasce "Smtp"; qualquer valor não reconhecido cai aqui, não
        // no arquivo — é a trava do risco 2 do plano §8 da 007.
        var emailService = ResolverEmailService("Sendgrid");

        Assert.IsType<EmailService>(emailService);
    }
}
