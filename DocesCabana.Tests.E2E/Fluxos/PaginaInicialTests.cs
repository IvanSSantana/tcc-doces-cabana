using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class PaginaInicialTests : TesteE2E
{
    public PaginaInicialTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_MenuAberto_Quando_CompararFundos_Entao_CategoriaDeveTerOMesmoFundoDoPainel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);
            await pagina.AbrirMenuDaCategoria();

            var fundoDoItem = await pagina.CorDeFundoDoLink();
            var fundoDoPainel = await pagina.CorDeFundoDoPainel();

            Assert.Equal(fundoDoPainel, fundoDoItem);
        });

    [Fact]
    public async Task Dado_MenuAberto_Quando_MedirOPainel_Entao_DeveTerALarguraDaFaixaDeConteudo() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);
            await pagina.AbrirMenuDaCategoria();

            var caixaDoPainel = await pagina.PainelDoMenu.BoundingBoxAsync();
            var caixaDaFaixa = await pagina.FaixaDeConteudo.BoundingBoxAsync();

            Assert.NotNull(caixaDoPainel);
            Assert.NotNull(caixaDaFaixa);
            Assert.Equal(caixaDaFaixa!.Width, caixaDoPainel!.Width, precision: 0);
        });

    [Fact]
    public async Task Dado_MenuAberto_Quando_CompararCartaoEPainel_Entao_CartaoDeveEstarRecuadoNosQuatroLados() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);
            await pagina.AbrirMenuDaCategoria();

            var caixaDoCartao = await pagina.CartaoDoMenu.BoundingBoxAsync();
            var caixaDoPainel = await pagina.PainelDoMenu.BoundingBoxAsync();

            Assert.NotNull(caixaDoCartao);
            Assert.NotNull(caixaDoPainel);
            Assert.True(caixaDoCartao!.X > caixaDoPainel!.X);
            Assert.True(caixaDoCartao.Y > caixaDoPainel.Y);
            Assert.True(caixaDoCartao.X + caixaDoCartao.Width < caixaDoPainel.X + caixaDoPainel.Width);
            Assert.True(caixaDoCartao.Y + caixaDoCartao.Height < caixaDoPainel.Y + caixaDoPainel.Height);
        });

    [Fact]
    public async Task Dado_NavegacaoPorTeclado_Quando_OFocoChegaNaCategoria_Entao_OMenuDeveAbrir() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            await pagina.LinkDaCategoria.FocusAsync();

            await Expect(pagina.PainelDoMenu).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CatalogoComDezenasDeProdutos_Quando_AbrirAPaginaInicial_Entao_AVitrineDeveRespeitarOLimite() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.CardsDaVitrine).ToHaveCountAsync(8);
        });

    [Fact]
    public async Task Dado_PaginaInicial_Quando_ContarOsPontosVisiveis_Entao_DeveHaverUmPorPosicaoAlcancavel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.PontosVisiveis).ToHaveCountAsync(5);
        });

    [Fact]
    public async Task Dado_PaginaInicial_Quando_LerOTituloDaSecao_Entao_NaoDeveDizerMaisVendidos() =>
        await Executar(async () =>
        {
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            var titulo = await pagina.TituloDaVitrine.InnerTextAsync();

            Assert.DoesNotContain("Mais Vendidos", titulo);
        });

    [Fact]
    public async Task Dado_TelaDe375px_Quando_AbrirAPaginaInicial_Entao_NaoDeveHaverRolagemHorizontal() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(375, 800);
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            // CA-08 mede o conteúdo, não o documento — o cabeçalho compartilhado
            // já estoura a 375px por conta própria desde a 009 (fora de escopo).
            var larguraDoConteudo = await Pagina.Locator("main, .pagina-inicial-vitrine").First
                .EvaluateAsync<double>("el => el.scrollWidth");
            var larguraDaTela = await Pagina.EvaluateAsync<double>("() => window.innerWidth");

            Assert.True(larguraDoConteudo <= larguraDaTela + 1);
        });
}
