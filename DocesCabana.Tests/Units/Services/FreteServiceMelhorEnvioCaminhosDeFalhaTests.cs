using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

public class FreteServiceMelhorEnvioCaminhosDeFalhaTests
{
    private static List<LinhaDoCarrinhoDTO> CriarItens() =>
    [
        new()
        {
            ProdutoId = Guid.NewGuid(),
            Nome = "Brigadeiro",
            PrecoUnitario = 10m,
            Quantidade = 1,
            ValorDaLinha = 10m,
            Peso = 0.5m,
            Altura = 10m,
            Largura = 15m,
            Comprimento = 20m
        }
    ];

    private static FreteServiceMelhorEnvio CriarServico(string urlBase, int timeoutEmSegundos = 10)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(urlBase) };
        var settings = Options.Create(new FreteSettings
        {
            UrlBase = urlBase,
            Token = "token-de-teste",
            CepDeOrigem = "17340001",
            UserAgent = "Doces Cabana (teste@docescabana.com.br)",
            TimeoutEmSegundos = timeoutEmSegundos
        });

        return new FreteServiceMelhorEnvio(httpClient, settings);
    }

    [Fact]
    public async Task Dado_ServidorInalcancavel_Quando_Cotar_Entao_DeveDevolverMensagemSemLancar()
    {
        var servico = CriarServico("http://localhost:9");

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(cotacao.Mensagem);
        Assert.Empty(cotacao.Opcoes);
    }

    [Fact]
    public async Task Dado_TimeoutMuitoCurto_Quando_Cotar_Entao_DeveDevolverMensagemSemLancar()
    {
        var servico = CriarServico("http://10.255.255.1", timeoutEmSegundos: 1);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(cotacao.Mensagem);
        Assert.Empty(cotacao.Opcoes);
    }
}
