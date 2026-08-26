using System.Net;
using System.Net.Http;
using DocesCabana.Application.DTOs;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

// Prova o mapeamento contra o exemplo real da documentação do MelhorEnvio
// (spec 020, plano §4) — sem credencial nenhuma: o HttpClient é apontado
// para um handler falso que devolve o JSON documentado, nunca a rede.
public class FreteServiceMelhorEnvioTests
{
    // Handler falso — só devolve o que o teste configurar, sem tocar rede.
    private sealed class HandlerFalso : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _corpo;
        public HttpRequestMessage? UltimaRequisicao { get; private set; }
        public string? UltimoCorpo { get; private set; }

        public HandlerFalso(HttpStatusCode status, string corpo)
        {
            _status = status;
            _corpo = corpo;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UltimaRequisicao = request;
            if (request.Content is not null)
                UltimoCorpo = await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_status) { Content = new StringContent(_corpo) };
        }
    }

    // Recorte do exemplo real de resposta da documentação (spec 020,
    // parte da conversa em que a documentação foi obtida) — PAC, SEDEX e
    // uma entrada sem custom_price utilizável, para provar o descarte.
    private const string RespostaDocumentada = """
    [
      {
        "id": 1,
        "name": "PAC",
        "price": "37.79",
        "custom_price": "37.79",
        "discount": "2.09",
        "currency": "R$",
        "delivery_time": 9,
        "delivery_range": { "min": 8, "max": 9 },
        "custom_delivery_time": 9,
        "custom_delivery_range": { "min": 8, "max": 9 },
        "company": { "id": 1, "name": "Correios", "picture": "https://sandbox.melhorenvio.com.br/images/shipping-companies/correios.png" }
      },
      {
        "id": 2,
        "name": "SEDEX",
        "price": "46.23",
        "custom_price": "46.23",
        "discount": "3.95",
        "currency": "R$",
        "delivery_time": 4,
        "delivery_range": { "min": 3, "max": 4 },
        "custom_delivery_time": 4,
        "custom_delivery_range": { "min": 3, "max": 4 },
        "company": { "id": 1, "name": "Correios", "picture": "https://sandbox.melhorenvio.com.br/images/shipping-companies/correios.png" }
      },
      {
        "id": 99,
        "name": "Serviço sem preço",
        "error": "Serviço indisponível para esse trecho",
        "company": { "id": 3, "name": "Transportadora X" }
      }
    ]
    """;

    private static FreteServiceMelhorEnvio CriarServico(HandlerFalso handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://sandbox.melhorenvio.com.br") };
        var settings = Options.Create(new FreteSettings
        {
            UrlBase = "https://sandbox.melhorenvio.com.br",
            Token = "token-de-teste",
            CepDeOrigem = "17340001",
            UserAgent = "Doces Cabana (teste@docescabana.com.br)",
            TimeoutEmSegundos = 10
        });

        return new FreteServiceMelhorEnvio(httpClient, settings);
    }

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

    [Fact]
    public async Task Dado_RespostaDocumentada_Quando_Cotar_Entao_DeveUsarCustomPriceENaoPrice()
    {
        // Armadilha 1 (plano §4): custom_price reflete taxas/descontos da
        // conta; price é o valor "de tabela", que não deve ser usado.
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        var pac = cotacao.Opcoes.Single(o => o.ServicoId == 1);
        Assert.Equal(37.79m, pac.Preco);
    }

    [Fact]
    public async Task Dado_RespostaDocumentada_Quando_Cotar_Entao_PrecoComPontoDecimalNaoDeveVirarMilhar()
    {
        // Armadilha 2 (plano §4): a aplicação é pt-BR — Parse sem
        // InvariantCulture transformaria "46.23" em 4623, não 46,23. É a
        // única das três armadilhas que passaria despercebida por qualquer
        // asserção relacional (preço > 0, distante custa mais).
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        var sedex = cotacao.Opcoes.Single(o => o.ServicoId == 2);
        Assert.Equal(46.23m, sedex.Preco);
        Assert.True(sedex.Preco < 100m, "o preço não pode ter virado milhar");
    }

    [Fact]
    public async Task Dado_RespostaDocumentada_Quando_Cotar_Entao_DeveUsarCustomDeliveryRangeENaoDeliveryTime()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        var pac = cotacao.Opcoes.Single(o => o.ServicoId == 1);
        Assert.Equal(8, pac.PrazoMinimoEmDias);
        Assert.Equal(9, pac.PrazoMaximoEmDias);
    }

    [Fact]
    public async Task Dado_RespostaDocumentada_Quando_Cotar_Entao_DeveMapearTransportadoraEServico()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        var pac = cotacao.Opcoes.Single(o => o.ServicoId == 1);
        Assert.Equal("Correios", pac.Transportadora);
        Assert.Equal("PAC", pac.Servico);
    }

    [Fact]
    public async Task Dado_EntradaSemPrecoUtilizavel_Quando_Cotar_Entao_DeveSerDescartada()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.DoesNotContain(cotacao.Opcoes, o => o.ServicoId == 99);
        Assert.Equal(2, cotacao.Opcoes.Count);
    }

    [Fact]
    public async Task Dado_RespostaValida_Quando_Cotar_Entao_NaoDeveTerMensagemDeFalha()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("01310000", CriarItens());

        Assert.Null(cotacao.Mensagem);
    }

    [Fact]
    public async Task Dado_QualquerRequisicao_Quando_Cotar_Entao_DeveEnviarUserAgentEAuthorization()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(handler.UltimaRequisicao);
        Assert.Equal("Bearer token-de-teste", handler.UltimaRequisicao!.Headers.Authorization?.ToString());
        Assert.True(handler.UltimaRequisicao.Headers.UserAgent.Count > 0, "User-Agent é obrigatório pela API");
    }

    [Fact]
    public async Task Dado_ItemDisponivel_Quando_Cotar_Entao_DeveEnviarInsuranceValueComOPrecoDoProduto()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, RespostaDocumentada);
        var servico = CriarServico(handler);

        await servico.Cotar("01310000", CriarItens());

        Assert.NotNull(handler.UltimoCorpo);
        Assert.Contains("\"insurance_value\":10", handler.UltimoCorpo);
    }

    [Fact]
    public async Task Dado_RespostaComErro422_Quando_Cotar_Entao_DeveDevolverMensagemSemLancar()
    {
        var corpo422 = """{"message":"The given data was invalid.","errors":{"to.postal_code":["O campo to.postal code é obrigatório."]}}""";
        var handler = new HandlerFalso(HttpStatusCode.UnprocessableEntity, corpo422);
        var servico = CriarServico(handler);

        var cotacao = await servico.Cotar("00000000", CriarItens());

        Assert.NotNull(cotacao.Mensagem);
        Assert.Empty(cotacao.Opcoes);
    }
}
