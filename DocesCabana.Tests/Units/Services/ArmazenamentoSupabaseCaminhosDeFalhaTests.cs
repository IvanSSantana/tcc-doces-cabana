using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

public class ArmazenamentoSupabaseCaminhosDeFalhaTests
{
    private static ArmazenamentoSupabase CriarServico(string urlBase, int timeoutEmSegundos = 10)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(urlBase) };
        var settings = Options.Create(new SupabaseSettings
        {
            UrlBase = urlBase,
            Bucket = "images",
            Pasta = "public",
            ChaveDeServico = "chave-de-teste",
            TimeoutEmSegundos = timeoutEmSegundos
        });

        return new ArmazenamentoSupabase(httpClient, settings);
    }

    [Fact]
    public async Task Dado_ServidorInalcancavel_Quando_Enviar_Entao_DeveDevolverFalhaSemLancar()
    {
        var servico = CriarServico("http://localhost:9");
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }

    [Fact]
    public async Task Dado_TimeoutMuitoCurto_Quando_Enviar_Entao_DeveDevolverFalhaSemLancar()
    {
        var servico = CriarServico("http://10.255.255.1", timeoutEmSegundos: 1);
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }
}
