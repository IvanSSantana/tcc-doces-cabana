using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

// Caminhos de falha do adaptador, sem mock nenhum (spec 020, plano §6) —
// aponta a configuração para um lugar que não responde, exercitando o
// HttpClient de verdade. Nenhum dos dois precisa da credencial: um servidor
// inalcançável e um timeout curto não pedem token válido para acontecer.
//
// A terceira falha do plano — token inválido devolvendo 401 — precisa de um
// servidor de verdade respondendo (não há como simular um 401 sem rede), e
// por isso fica para a Fase 8 (T048), marcada [Trait("Categoria",
// "Externo")]: rodar aqui, fora desse filtro, faria a suíte inteira
// depender da disponibilidade do MelhorEnvio para passar — contra o que o
// plano promete em §9 ("dotnet test continua verde sem rede").
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
        // Porta que ninguém escuta em localhost — conexão recusada de
        // verdade, sem depender de nenhum serviço externo estar no ar.
        var servico = CriarServico("http://localhost:9");

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(cotacao.Mensagem);
        Assert.Empty(cotacao.Opcoes);
    }

    [Fact]
    public async Task Dado_TimeoutMuitoCurto_Quando_Cotar_Entao_DeveDevolverMensagemSemLancar()
    {
        // Endereço não roteável (RFC 5737/documentação) — a conexão nunca
        // completa, e o timeout de 1s do CancellationTokenSource interno
        // estoura antes de qualquer resposta chegar.
        var servico = CriarServico("http://10.255.255.1", timeoutEmSegundos: 1);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(cotacao.Mensagem);
        Assert.Empty(cotacao.Opcoes);
    }
}
