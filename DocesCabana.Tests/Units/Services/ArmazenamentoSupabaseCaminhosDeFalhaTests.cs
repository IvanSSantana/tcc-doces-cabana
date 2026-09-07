using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

// Caminhos de falha do adaptador, sem mock nenhum (spec 027, plano §7) —
// mesmo par que FreteServiceMelhorEnvioCaminhosDeFalhaTests usa, e pela
// mesma razão: a suíte padrão não pode depender de rede.
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
        // Porta que ninguém escuta em localhost — conexão recusada de
        // verdade, sem depender de nenhum serviço externo estar no ar.
        var servico = CriarServico("http://localhost:9");
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }

    [Fact]
    public async Task Dado_TimeoutMuitoCurto_Quando_Enviar_Entao_DeveDevolverFalhaSemLancar()
    {
        // Endereço não roteável (RFC 5737/documentação) — a conexão nunca
        // completa, e o timeout de 1s estoura antes de qualquer resposta.
        var servico = CriarServico("http://10.255.255.1", timeoutEmSegundos: 1);
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
    }
}
