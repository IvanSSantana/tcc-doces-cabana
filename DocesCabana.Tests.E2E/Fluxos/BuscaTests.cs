using System.Text.RegularExpressions;
using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

// Busca por nome de produto (spec 016) — CA-01 a CA-11. A barra vive no
// cabeçalho, presente em qualquer página; o resultado é o catálogo comum,
// filtrado por termo.
public class BuscaTests : TesteE2E
{
    public BuscaTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_BarraDePesquisa_Quando_BuscarPeloNome_Entao_DeveMostrarOProduto() =>
        await Executar(async () =>
        {
            var inicial = new PaginaInicial(Pagina);
            await inicial.Abrir(UrlBase);

            await inicial.Buscar("Raspa Tacho");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.Cards).ToHaveCountAsync(1);
        });

    [Fact]
    public async Task Dado_CatalogoDeUmaCategoria_Quando_BuscarProdutoDeOutra_Entao_DeveEncontrar() =>
        await Executar(async () =>
        {
            var catalogo = new PaginaCatalogo(Pagina);
            // Dentro de "Doces" — "Café" só existe em "Empório" (DbInitializer).
            await catalogo.Abrir(UrlBase, "doces");

            await catalogo.BarraDePesquisa.FillAsync("Café");
            await Pagina.Locator(".botao-pesquisar").ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new Regex(@"^http://[^/]+/Catalogo\?"));
            var contagem = await catalogo.Cards.CountAsync();
            Assert.True(contagem > 0);
        });

    [Fact]
    public async Task Dado_ProdutoComAcento_Quando_BuscarSemAcentoEEmOutraCaixa_Entao_DeveEncontrar() =>
        await Executar(async () =>
        {
            var inicial = new PaginaInicial(Pagina);
            await inicial.Abrir(UrlBase);

            await inicial.Buscar("CAFE");

            var catalogo = new PaginaCatalogo(Pagina);
            var contagem = await catalogo.Cards.CountAsync();
            Assert.True(contagem > 0);
        });

    [Fact]
    public async Task Dado_ResultadoDeBusca_Quando_OlharATela_Entao_DeveTerOrdenacaoPaginacaoEBarraLateralECartao() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=cafe&ordenacao=NomeAZ");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.SeletorDeOrdenacao).ToBeVisibleAsync();
            await Expect(catalogo.Categorias.First).ToBeVisibleAsync();
            await Expect(catalogo.Cards.First).ToBeVisibleAsync();
            // Favorito é parte do cartão de sempre — mesmo componente,
            // nenhuma tela nova (RF-04).
            await Expect(catalogo.Cards.First.Locator(".botao-favorito-card")).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_BuscaFeita_Quando_TrocarOrdenacao_Entao_OTermoDeveSobreviver() =>
        await Executar(async () =>
        {
            var catalogo = new PaginaCatalogo(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=cafe&ordenacao=NomeAZ");

            await catalogo.SeletorDeOrdenacao.SelectOptionAsync(new Microsoft.Playwright.SelectOptionValue { Label = "Menor preço" });
            await Pagina.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

            await Expect(Pagina).ToHaveURLAsync(new Regex("termo=cafe"));
        });

    [Fact]
    public async Task Dado_BuscaFeita_Quando_EscolherCategoria_Entao_OTermoDeveSobreviver() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=cafe&ordenacao=NomeAZ");

            var catalogo = new PaginaCatalogo(Pagina);
            await catalogo.LinkDeCategoria("Empório").ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new Regex(@"/Catalogo/emporio\?.*termo=cafe"));
        });

    [Fact]
    public async Task Dado_BuscaFeita_Quando_OlharABarraDePesquisa_Entao_DeveConterOTermo() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=brigadeiro");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.BarraDePesquisa).ToHaveValueAsync("brigadeiro");
        });

    [Fact]
    public async Task Dado_BuscaComCategoriaEscolhida_Quando_DesfazerABusca_Entao_DeveManterACategoria() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/emporio?termo=cafe");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.EtiquetaDeBusca).ToBeVisibleAsync();

            await catalogo.BotaoRemoverBusca.ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new Regex(@"/Catalogo/emporio(\?(?!.*termo=).*)?$"));
            await Expect(catalogo.CategoriaAtiva).ToHaveTextAsync("Empório");
        });

    [Fact]
    public async Task Dado_TermoSemResultado_Quando_OlharOResultado_Entao_DeveMencionarOTermoBuscado() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=xyzxyznaoexiste123");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.MensagemVazia).ToContainTextAsync("xyzxyznaoexiste123");
            await Expect(catalogo.MensagemVazia).Not.ToContainTextAsync("sem açúcar");
        });

    [Fact]
    public async Task Dado_BarraVazia_Quando_Submeter_Entao_DeveMostrarOCatalogoCompleto() =>
        await Executar(async () =>
        {
            var inicial = new PaginaInicial(Pagina);
            await inicial.Abrir(UrlBase);

            await inicial.Buscar("");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_Buscar_Entao_DeveMostrarOResultado() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            var inicial = new PaginaInicial(paginaSemScript);
            await inicial.Abrir(UrlBase);
            await inicial.Buscar("Raspa Tacho");

            var catalogo = new PaginaCatalogo(paginaSemScript);
            await Expect(catalogo.Cards).ToHaveCountAsync(1);
        });

    [Fact]
    public async Task Dado_ProdutoForaDoCatalogoPublico_Quando_BuscarPeloNomeExato_Entao_NaoDeveAparecer() =>
        await Executar(async () =>
        {
            // "Bolachas / Rosquinhas 2" é o produto Inativo semeado pelo
            // DbInitializer (spec 012, CA-20) — RN-06 vale também na busca.
            await Pagina.GotoAsync($"{UrlBase}/Catalogo?termo=" + Uri.EscapeDataString("Bolachas / Rosquinhas 2"));

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.Cards).ToHaveCountAsync(0);
        });
}
