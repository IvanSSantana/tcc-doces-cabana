using System.Net;
using System.Net.Http;
using DocesCabana.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DocesCabana.Tests.Units.Services;

// Prova o adaptador contra um handler falso, sem tocar a rede (spec 027,
// plano §7) — mesmo padrão de FreteServiceMelhorEnvioTests.
public class ArmazenamentoSupabaseTests
{
    // Handler falso — só devolve o que o teste configurar, sem tocar rede.
    private sealed class HandlerFalso : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _corpo;
        public HttpRequestMessage? UltimaRequisicao { get; private set; }
        public int Chamadas { get; private set; }

        public HandlerFalso(HttpStatusCode status, string corpo)
        {
            _status = status;
            _corpo = corpo;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Chamadas++;
            UltimaRequisicao = request;
            return Task.FromResult(new HttpResponseMessage(_status) { Content = new StringContent(_corpo) });
        }
    }

    private static ArmazenamentoSupabase CriarServico(HandlerFalso handler, string chaveDeServico = "chave-de-teste")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://projeto.supabase.co") };
        var settings = Options.Create(new SupabaseSettings
        {
            UrlBase = "https://projeto.supabase.co",
            Bucket = "images",
            Pasta = "public",
            ChaveDeServico = chaveDeServico,
            TimeoutEmSegundos = 10
        });

        return new ArmazenamentoSupabase(httpClient, settings);
    }

    [Fact]
    public async Task Dado_EnvioBemSucedido_Quando_Enviar_Entao_DeveDevolverEnderecoPublicoMontadoCorretamente()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, "{}");
        var servico = CriarServico(handler);
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.True(resultado.Sucesso);
        Assert.StartsWith("https://projeto.supabase.co/storage/v1/object/public/images/public/", resultado.Url);
        Assert.EndsWith(".jpg", resultado.Url);
    }

    [Fact]
    public async Task Dado_NomeOriginalComCaminhoEAcento_Quando_Enviar_Entao_OCaminhoNaoContemONomeRecebido()
    {
        // RN-02/RF-07, CA-07: quem nomeia o arquivo guardado é o adaptador —
        // um Guid, nunca o nome que veio do computador de quem enviou.
        var handler = new HandlerFalso(HttpStatusCode.OK, "{}");
        var servico = CriarServico(handler);
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "../Relatório Confidencial 2026.png", "image/png");

        Assert.True(resultado.Sucesso);
        Assert.DoesNotContain("Relatório", resultado.Url);
        Assert.DoesNotContain("Confidencial", resultado.Url);
        Assert.EndsWith(".png", resultado.Url);
        Assert.True(Guid.TryParse(
            Path.GetFileNameWithoutExtension(resultado.Url!.Split('/').Last()), out _));
    }

    [Fact]
    public async Task Dado_EnvioBemSucedido_Quando_Enviar_Entao_DeveEnviarAuthorizationEContentType()
    {
        var handler = new HandlerFalso(HttpStatusCode.OK, "{}");
        var servico = CriarServico(handler);
        using var conteudo = new MemoryStream([1, 2, 3]);

        await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.NotNull(handler.UltimaRequisicao);
        Assert.Equal("Bearer chave-de-teste", handler.UltimaRequisicao!.Headers.Authorization?.ToString());
        Assert.Equal("image/jpeg", handler.UltimaRequisicao.Content?.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Dado_RespostaDeErro_Quando_Enviar_Entao_DeveDevolverFalhaComMensagemSemLancar(HttpStatusCode status)
    {
        var handler = new HandlerFalso(status, """{"message":"erro"}""");
        var servico = CriarServico(handler);
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
        Assert.Null(resultado.Url);
    }

    [Fact]
    public async Task Dado_ChaveDeServicoEmBranco_Quando_Enviar_Entao_DeveRecusarSemFazerRequisicaoNenhuma()
    {
        // CA-09: determinístico e offline — a chave vazia nunca toca a rede.
        var handler = new HandlerFalso(HttpStatusCode.OK, "{}");
        var servico = CriarServico(handler, chaveDeServico: "");
        using var conteudo = new MemoryStream([1, 2, 3]);

        var resultado = await servico.Enviar(conteudo, "brigadeiro.jpg", "image/jpeg");

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.Mensagem);
        Assert.Equal(0, handler.Chamadas);
    }
}
