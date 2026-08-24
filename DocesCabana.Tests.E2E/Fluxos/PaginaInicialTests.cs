using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Data.Sqlite;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class PaginaInicialTests : TesteE2E
{
    public PaginaInicialTests(FixtureE2E fixture) : base(fixture) { }

    private async Task EntrarComoClienteSeed()
    {
        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);
    }

    // Mesmo ícone "favoritado" que FavoritosTests usa (spec 015, plano §7):
    // o kit da FontAwesome converte <i> em <svg> ao carregar, preservando
    // classes que não reconhece — checar as duas formas cobre antes e
    // depois da conversão.
    private static ILocator IconeFavoritado(ILocator botao) =>
        botao.Locator("svg.favoritado, i.favoritado");

    private static async Task EsperarFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(1);

    private static async Task EsperarNaoFavoritado(ILocator botao) =>
        await Expect(IconeFavoritado(botao)).ToHaveCountAsync(0);

    // Nota média por produto, a mesma leitura que ProdutoRepository faz para
    // ordenar (AVG(Nota), nulo quando não há avaliação — plano §5).
    private double ObterNotaMedia(Guid produtoId)
    {
        using var conexao = new SqliteConnection($"Data Source={Aplicacao.CaminhoDoBanco}");
        conexao.Open();
        using var comando = conexao.CreateCommand();
        comando.CommandText = "SELECT AVG(Nota) FROM Avaliacao WHERE UPPER(ProdutoId) = UPPER($id)";
        comando.Parameters.AddWithValue("$id", produtoId.ToString());
        var resultado = comando.ExecuteScalar();
        return resultado is DBNull or null ? -1 : Convert.ToDouble(resultado);
    }

    private void AlterarStatusDoProduto(Guid produtoId, byte status)
    {
        using var conexao = new SqliteConnection($"Data Source={Aplicacao.CaminhoDoBanco}");
        conexao.Open();
        using var comando = conexao.CreateCommand();
        comando.CommandText = "UPDATE Produto SET Status = $status WHERE UPPER(ProdutoId) = UPPER($id)";
        comando.Parameters.AddWithValue("$status", status);
        comando.Parameters.AddWithValue("$id", produtoId.ToString());
        comando.ExecuteNonQuery();
    }

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
    public async Task Dado_PaginaInicial_Quando_LerOTituloDaSecao_Entao_DeveDizerBemAvaliados() =>
        await Executar(async () =>
        {
            // CA-10
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            var titulo = await pagina.TituloDaVitrine.InnerTextAsync();

            Assert.Contains("Bem avaliados", titulo);
        });

    [Fact]
    public async Task Dado_ProdutosComAvaliacoesDiferentes_Quando_AbrirAPaginaInicial_Entao_AVitrineDeveOrdenarPorNotaMedia() =>
        await Executar(async () =>
        {
            // CA-07/CA-08: em vez de fabricar notas, lê a ordem real exibida
            // e confere contra a mesma leitura que ProdutoRepository faz
            // para ordenar (AVG(Nota) ?? -1, decrescente) — prova que a
            // vitrine aplica o critério, não que um dado específico existe.
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            var quantidade = await pagina.CardsDaVitrine.CountAsync();
            var idsNaOrdemExibida = new List<Guid>();
            for (var i = 0; i < quantidade; i++)
            {
                var texto = await pagina.CardsDaVitrine.Nth(i).GetAttributeAsync("data-produto-id");
                idsNaOrdemExibida.Add(Guid.Parse(texto!));
            }

            var notasNaOrdemExibida = idsNaOrdemExibida.Select(ObterNotaMedia).ToList();
            var notasEmOrdemDecrescente = notasNaOrdemExibida.OrderByDescending(n => n).ToList();

            Assert.Equal(notasEmOrdemDecrescente, notasNaOrdemExibida);
        });

    [Fact]
    public async Task Dado_ProdutoQueEstavaNaVitrine_Quando_FicarForaDoCatalogo_Entao_NaoDeveMaisAparecer() =>
        await Executar(async () =>
        {
            // CA-09
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            var idTexto = await pagina.CardsDaVitrine.First.GetAttributeAsync("data-produto-id");
            var produtoId = Guid.Parse(idTexto!);

            // ProdutoStatus.Inativo = 0 (Domain/Enums/ProdutoStatus.cs).
            AlterarStatusDoProduto(produtoId, 0);

            try
            {
                await pagina.Abrir(UrlBase);

                var idsAposFicarInativo = await pagina.CardsDaVitrine.EvaluateAllAsync<string[]>(
                    "cards => cards.map(c => c.getAttribute('data-produto-id'))");

                Assert.DoesNotContain(
                    idsAposFicarInativo, id => Guid.Parse(id!) == produtoId);
            }
            finally
            {
                AlterarStatusDoProduto(produtoId, 1); // Ativo de novo
            }
        });

    [Fact]
    public async Task Dado_FavoritoNaVitrine_Quando_RecarregarAPaginaInicial_Entao_DeveContinuarMarcado() =>
        await Executar(async () =>
        {
            // CA-11 — o defeito real que a spec 019 descreve: o coração
            // nasce sempre vazio porque a home nunca perguntava quais
            // produtos a pessoa favoritou.
            await EntrarComoClienteSeed();
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            var botao = pagina.BotaoFavoritar();

            if (await IconeFavoritado(botao).CountAsync() == 0)
            {
                await botao.ClickAsync();
                await EsperarFavoritado(botao);
            }

            await pagina.Abrir(UrlBase);
            var botaoAposRecarga = pagina.BotaoFavoritar();
            await EsperarFavoritado(botaoAposRecarga);

            // Desfaz, para não deixar rastro para os testes seguintes.
            await botaoAposRecarga.ClickAsync();
            await EsperarNaoFavoritado(botaoAposRecarga);
        });

    [Fact]
    public async Task Dado_VisitanteSemAutenticacao_Quando_AbrirAPaginaInicial_Entao_NenhumProdutoDeveAparecerFavoritado() =>
        await Executar(async () =>
        {
            // CA-12
            var pagina = new PaginaInicial(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.CardsDaVitrine.Locator("svg.favoritado, i.favoritado")).ToHaveCountAsync(0);
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
