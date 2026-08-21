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

    // Cliente 8 não é usado por nenhum outro teste desta classe — os testes
    // de lista vazia e de acesso negado precisam de alguém sem rastro de
    // favorito nenhum, e o cliente1 pode carregar sobra de outros testes.
    private const string EmailClienteSemFavoritos = "cliente8.seed@docescabana.com.br";

    private async Task Entrar(string email, IPage? pagina = null)
    {
        var alvo = pagina ?? Pagina;
        var paginaLogin = new PaginaLogin(alvo);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(email, AplicacaoEmExecucao.SenhaClienteSeed);
    }

    // O kit da FontAwesome (_Footer.cshtml) converte <i> em <svg> ao
    // carregar a página e apaga a tag original — um seletor que procura só
    // "i" nunca resolve depois disso. FA preserva classes que não reconhece
    // (como "favoritado") no elemento convertido, então checar as duas
    // formas cobre antes e depois da conversão (e o caso sem JavaScript,
    // onde a conversão nunca acontece).
    private static ILocator IconeFavoritado(ILocator botao) =>
        botao.Locator("svg.favoritado, i.favoritado");

    private static async Task<bool> EstaFavoritadoAgora(ILocator botao) =>
        await IconeFavoritado(botao).CountAsync() > 0;

    // NetworkIdle marca o fim da requisição de rede, não o fim do .then()
    // que troca o ícone — há uma folga real entre as duas. Esperar pelo
    // resultado visível (com o retry automático do Expect), em vez de ler
    // o estado uma vez só, é o que evita essa corrida.
    private static async Task EsperarFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(1);

    private static async Task EsperarNaoFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(0);

    // Ordenação fixada em Nome (A-Z): torna "o primeiro card" sempre o mesmo
    // produto, independente da ordenação padrão por avaliação (spec 014).
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

            // Garante o estado inicial, independente de execuções anteriores
            // terem deixado o produto favoritado (a base é compartilhada
            // pela suíte inteira).
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

            // A mesma ordenação de antes — sem fixá-la, a página recarrega
            // com o padrão (melhor avaliados) e "o primeiro card" deixa de
            // ser o produto que acabamos de favoritar.
            pagina = await AbrirCatalogoDeDoces(Pagina);
            var botaoAposRecarga = pagina.Cards.First.Locator(".botao-favorito-card");
            await EsperarFavoritado(botaoAposRecarga);

            // Desfaz, para não deixar rastro para os testes seguintes.
            await botaoAposRecarga.ClickAsync();
            await EsperarNaoFavoritado(botaoAposRecarga);
        });

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_Favoritar_Entao_NaoDeveRecarregarAPagina() =>
        await Executar(async () =>
        {
            await EntrarComoClienteSeed();
            var pagina = await AbrirCatalogoDeDoces(Pagina);

            // Marca um elemento fora da grade de produtos — se a página
            // recarregar, ele desaparece do DOM e a leitura seguinte falha.
            await Pagina.EvaluateAsync("() => { document.body.dataset.marcadorDeRecarga = 'presente'; }");

            var botao = pagina.Cards.First.Locator(".botao-favorito-card");
            await botao.ClickAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var marcador = await Pagina.EvaluateAsync<string?>("() => document.body.dataset.marcadorDeRecarga");
            Assert.Equal("presente", marcador);

            // Desfaz.
            await botao.ClickAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_Favoritar_Entao_DeveFuncionarEVoltarAListagem() =>
        await Executar(async () =>
        {
            // RF-04/CA-05: o coração é um botão de envio associado a um
            // formulário comum — sem script, ele posta e o servidor
            // redireciona de volta (spec 015, plano §1).
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

            // Desfaz.
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

            // Desfaz pela própria lista — cobre parte do caminho de CA-11
            // de propósito, para não deixar rastro.
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
            // Achado de verificação ao vivo (T059): a mensagem de "nenhum
            // favorito" só existia quando o servidor já renderizava a lista
            // vazia — esvaziar por aqui (desfavoritar o último item) nunca
            // a mostrava. Usa o cliente sem rastro para garantir que o
            // último item realmente esvazia a lista.
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
            // Cliente próprio deste teste — precisa começar sem favorito
            // nenhum, e nenhum outro teste desta classe o usa.
            const string emailDoVisitante = "cliente7.seed@docescabana.com.br";

            var pagina = await AbrirCatalogoDeDoces(Pagina);
            var botao = pagina.Cards.First.Locator(".botao-favorito-card");

            await botao.ClickAsync();
            await Expect(Pagina.Locator("#modal-login")).ToBeVisibleAsync();

            await Pagina.Locator("#modal-login .botao-entrar").ClickAsync();

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Entrar(emailDoVisitante, AplicacaoEmExecucao.SenhaClienteSeed);

            // O login devolve à mesma página do catálogo (RF-13), e a
            // intenção pendente se conclui sozinha ao carregar (RF-07).
            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Catalogo/doces"));
            var botaoAposLogin = pagina.Cards.First.Locator(".botao-favorito-card");
            await EsperarFavoritado(botaoAposLogin);

            // Desfaz.
            await botaoAposLogin.ClickAsync();
            await EsperarNaoFavoritado(botaoAposLogin);
        });

    [Fact]
    public async Task Dado_TelaSensivelAoToque_Quando_AbrirOCatalogo_Entao_OControleDeveEstarVisivel() =>
        await Executar(async () =>
        {
            // RF-05/CA-06: sem :hover em tela de toque, o coração não pode
            // depender de passagem de mouse para aparecer (spec 015, plano §3).
            await using var contextoDeToque = await Navegador.NewContextAsync(new()
            {
                HasTouch = true,
                IsMobile = true,
                ViewportSize = new ViewportSize { Width = 390, Height = 844 },
            });
            var paginaDeToque = await contextoDeToque.NewPageAsync();
            await paginaDeToque.GotoAsync($"{UrlBase}/Catalogo/doces");

            // ToBeVisibleAsync não basta: o botão tem caixa delimitadora
            // mesmo com opacity: 0 (é assim que o :hover do desktop o
            // esconde) — Playwright considera isso "visível". A prova real
            // é a opacidade computada, sem que ninguém tenha passado o mouse.
            var botao = paginaDeToque.Locator(".card-produto").First.Locator(".botao-favorito-card");
            var opacidade = await botao.EvaluateAsync<string>("el => getComputedStyle(el).opacity");
            Assert.Equal("1", opacidade);
        });
}
