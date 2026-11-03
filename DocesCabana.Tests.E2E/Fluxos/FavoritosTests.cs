using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class FavoritosTests : TesteE2E
{
    public FavoritosTests(FixtureE2E fixture) : base(fixture) { }

    private async Task EntrarComoClienteSeed(IPage? pagina = null) =>
        await Entrar(AplicacaoEmExecucao.EmailClienteSeed, pagina);

    private const string EmailClienteSemFavoritos = "cliente8.seed@docescabana.com.br";

    private async Task Entrar(string email, IPage? pagina = null)
    {
        var alvo = pagina ?? Pagina;
        var paginaLogin = new PaginaLogin(alvo);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(email, AplicacaoEmExecucao.SenhaClienteSeed);
    }

    private static ILocator IconeFavoritado(ILocator botao) =>
        botao.Locator("svg.favoritado, i.favoritado");

    private static async Task<bool> EstaFavoritadoAgora(ILocator botao) =>
        await IconeFavoritado(botao).CountAsync() > 0;

    private static async Task EsperarFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(1);

    private static async Task EsperarNaoFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(0);

    private async Task<PaginaCatalogo> AbrirCatalogoDeDoces(IPage pagina)
    {
        var paginaCatalogo = new PaginaCatalogo(pagina);
        await pagina.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=NomeAZ");
        return paginaCatalogo;
    }

    [Fact]
    public async Task Dado_ProdutoNaoFavoritado_Quando_Favoritar_Entao_DeveMarcarEDesmarcarNoMesmoControle() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var pagina = await AbrirCatalogoDeDoces(Pagina);

            var botao = pagina.Cards.First.Locator(".botao-favorito-card");

            if (await EstaFavoritadoAgora(botao))
            {
                await botao.ClickAsync();
                await EsperarNaoFavoritado(botao);
            }

            await botao.ClickAsync();
            await EsperarFavoritado(botao);

            await botao.ClickAsync();
            await EsperarNaoFavoritado(botao);
        });

    [Fact]
    public async Task Dado_ProdutoFavoritado_Quando_RecarregarOCatalogo_Entao_DeveContinuarMarcado() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var pagina = await AbrirCatalogoDeDoces(Pagina);

            var botao = pagina.Cards.First.Locator(".botao-favorito-card");

            if (!await EstaFavoritadoAgora(botao))
            {
                await botao.ClickAsync();
                await EsperarFavoritado(botao);
            }

            pagina = await AbrirCatalogoDeDoces(Pagina);
            var botaoAposRecarga = pagina.Cards.First.Locator(".botao-favorito-card");
            await EsperarFavoritado(botaoAposRecarga);

            await botaoAposRecarga.ClickAsync();
            await EsperarNaoFavoritado(botaoAposRecarga);
        });

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_Favoritar_Entao_NaoDeveRecarregarAPagina() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var pagina = await AbrirCatalogoDeDoces(Pagina);

            await Pagina.EvaluateAsync("() => { document.body.dataset.marcadorDeRecarga = 'presente'; }");

            var botao = pagina.Cards.First.Locator(".botao-favorito-card");
            await botao.ClickAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var marcador = await Pagina.EvaluateAsync<string?>("() => document.body.dataset.marcadorDeRecarga");
            Assert.Equal("presente", marcador);

            await botao.ClickAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_Favoritar_Entao_DeveFuncionarEVoltarAListagem() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            await EntrarComoClienteSeed(paginaSemScript);
            var pagina = await AbrirCatalogoDeDoces(paginaSemScript);

            var botao = pagina.Cards.First.Locator(".botao-favorito-card");
            var estadoInicial = await EstaFavoritadoAgora(botao);

            await botao.ClickAsync();

            await Expect(paginaSemScript.Locator(".pagina-catalogo")).ToBeVisibleAsync();
            var botaoAposRedirecionamento = pagina.Cards.First.Locator(".botao-favorito-card");
            var estadoFinal = await EstaFavoritadoAgora(botaoAposRedirecionamento);
            Assert.NotEqual(estadoInicial, estadoFinal);

            await botaoAposRedirecionamento.ClickAsync();
        });

    [Fact]
    public async Task Dado_ProdutosFavoritados_Quando_AbrirALista_Entao_DeveMostrarExatamenteEles() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var catalogo = await AbrirCatalogoDeDoces(Pagina);

            var nomeEsperado = (await catalogo.Cards.First.Locator(".nome-card").TextContentAsync())!.Trim();
            var botaoNoCatalogo = catalogo.Cards.First.Locator(".botao-favorito-card");

            if (!await EstaFavoritadoAgora(botaoNoCatalogo))
            {
                await botaoNoCatalogo.ClickAsync();
                await EsperarFavoritado(botaoNoCatalogo);
            }

            var favoritos = new PaginaFavoritos(Pagina);
            await favoritos.Abrir(UrlBase);

            var cardNaLista = favoritos.Cards.Filter(new() { HasText = nomeEsperado });
            await Expect(cardNaLista).ToHaveCountAsync(1);

            await cardNaLista.Locator(".botao-favorito-card").ClickAsync();
        });

    [Fact]
    public async Task Dado_ListaDeFavoritos_Quando_Desfavoritar_Entao_DeveSairDaListaSemRecarregar() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var catalogo = await AbrirCatalogoDeDoces(Pagina);

            var botaoNoCatalogo = catalogo.Cards.First.Locator(".botao-favorito-card");
            if (!await EstaFavoritadoAgora(botaoNoCatalogo))
            {
                await botaoNoCatalogo.ClickAsync();
                await EsperarFavoritado(botaoNoCatalogo);
            }

            var favoritos = new PaginaFavoritos(Pagina);
            await favoritos.Abrir(UrlBase);

            var totalAntes = await favoritos.Cards.CountAsync();
            Assert.True(totalAntes > 0, "Precisa de ao menos um favorito para provar que ele sai da lista.");

            await favoritos.Cards.First.Locator(".botao-favorito-card").ClickAsync();

            await Expect(favoritos.Cards).ToHaveCountAsync(totalAntes - 1);
        });

    [Fact]
    public async Task Dado_UltimoFavorito_Quando_Desfavoritar_Entao_DeveMostrarAMensagemDeVazioSemRecarregar() =>
        await Executar(async () =>
        {
            await Entrar(EmailClienteSemFavoritos);
            var catalogo = await AbrirCatalogoDeDoces(Pagina);

            var botaoNoCatalogo = catalogo.Cards.First.Locator(".botao-favorito-card");
            if (!await EstaFavoritadoAgora(botaoNoCatalogo))
            {
                await botaoNoCatalogo.ClickAsync();
                await EsperarFavoritado(botaoNoCatalogo);
            }

            var favoritos = new PaginaFavoritos(Pagina);
            await favoritos.Abrir(UrlBase);
            await Expect(favoritos.Cards).ToHaveCountAsync(1);
            await Expect(favoritos.MensagemVazia).Not.ToBeVisibleAsync();

            await favoritos.Cards.First.Locator(".botao-favorito-card").ClickAsync();

            await Expect(favoritos.Cards).ToHaveCountAsync(0);
            await Expect(favoritos.MensagemVazia).ToBeVisibleAsync();
            await Expect(favoritos.LinkParaOCatalogo).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_NenhumFavorito_Quando_AbrirALista_Entao_DeveOferecerCaminhoParaOCatalogo() =>
        await Executar(async () =>
        {
            await Entrar(EmailClienteSemFavoritos);

            var favoritos = new PaginaFavoritos(Pagina);
            await favoritos.Abrir(UrlBase);

            await Expect(favoritos.Cards).ToHaveCountAsync(0);
            await Expect(favoritos.MensagemVazia).ToBeVisibleAsync();
            await Expect(favoritos.LinkParaOCatalogo).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Visitante_Quando_AbrirALista_Entao_DeveSerLevadoAEntrar() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Favorito");

            Assert.Contains("/Autenticacao/Login", Pagina.Url);
        });

    [Fact]
    public async Task Dado_Visitante_Quando_TentarFavoritar_Entao_DeveSerConvidadoAEntrarSemGravar() =>
        await Executar(async () =>
        {
            var pagina = await AbrirCatalogoDeDoces(Pagina);
            var botao = pagina.Cards.First.Locator(".botao-favorito-card");

            await botao.ClickAsync();

            await Expect(Pagina.Locator("#modal-login")).ToBeVisibleAsync();
            Assert.False(await EstaFavoritadoAgora(botao));
        });

    [Fact]
    public async Task Dado_VisitanteQueTentouFavoritar_Quando_Entrar_Entao_OProdutoDeveEstarFavoritado() =>
        await Executar(async () =>
        {
            const string emailDoVisitante = "cliente7.seed@docescabana.com.br";

            var pagina = await AbrirCatalogoDeDoces(Pagina);
            var botao = pagina.Cards.First.Locator(".botao-favorito-card");

            await botao.ClickAsync();
            await Expect(Pagina.Locator("#modal-login")).ToBeVisibleAsync();

            await Pagina.Locator("#modal-login .botao-entrar").ClickAsync();

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Entrar(emailDoVisitante, AplicacaoEmExecucao.SenhaClienteSeed);

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Catalogo/doces"));
            var botaoAposLogin = pagina.Cards.First.Locator(".botao-favorito-card");
            await EsperarFavoritado(botaoAposLogin);

            await botaoAposLogin.ClickAsync();
            await EsperarNaoFavoritado(botaoAposLogin);
        });

    [Fact]
    public async Task Dado_TelaSensivelAoToque_Quando_AbrirOCatalogo_Entao_OControleDeveEstarVisivel() =>
        await Executar(async () =>
        {
            await using var contextoDeToque = await Navegador.NewContextAsync(new()
            {
                HasTouch = true,
                IsMobile = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            });
            var paginaDeToque = await contextoDeToque.NewPageAsync();
            await paginaDeToque.GotoAsync($"{UrlBase}/Catalogo/doces");

            var botao = paginaDeToque.Locator(".card-produto").First.Locator(".botao-favorito-card");
            var opacidade = await botao.EvaluateAsync<string>("el => getComputedStyle(el).opacity");
            Assert.Equal("1", opacidade);
        });
}
