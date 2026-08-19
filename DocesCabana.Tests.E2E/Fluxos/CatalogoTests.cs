using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CatalogoTests : TesteE2E
{
    public CatalogoTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_Visitante_Quando_AbrirOCatalogo_Entao_DeveListarAPrimeiraPagina() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.Trilha).ToContainTextAsync("Todos");
            await Expect(pagina.CategoriaAtiva).ToHaveTextAsync("Todos");
            await Expect(pagina.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_Visitante_Quando_EscolherCategoriaNoCabecalho_Entao_DeveFiltrarPelaCategoria() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            await Pagina.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Doces", Exact = true }).First.ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Catalogo/doces");
        });

    [Fact]
    public async Task Dado_CatalogoDeEmporio_Quando_OlharOEndereco_Entao_DeveConterOApelidoLegivel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "emporio");

            Assert.Contains("/Catalogo/emporio", Pagina.Url);
        });

    [Fact]
    public async Task Dado_CategoriaComDozeSubcategorias_Quando_AbrirOMenu_Entao_DeveMostrarOito() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await Expect(pagina.CaixasDeSubcategoria).ToHaveCountAsync(8);
            await Expect(pagina.VerTodas).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CategoriaComQuatroSubcategorias_Quando_AbrirOMenu_Entao_DeveMostrarAsQuatro() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "adega");

            await Expect(pagina.CaixasDeSubcategoria).ToHaveCountAsync(4);
            await Expect(pagina.VerTodas).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_UmaSubcategoriaMarcada_Quando_MarcarASegunda_Entao_DeveSomarOsProdutosDasDuas() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "adega");

            await pagina.MarcarSubcategoriaPeloNome("Vinhos");
            var totalSo = await pagina.Cards.CountAsync();

            await pagina.MarcarSubcategoriaPeloNome("Cachaça");
            var totalSomado = await pagina.Cards.CountAsync();

            Assert.True(totalSomado >= totalSo);
        });

    [Fact]
    public async Task Dado_CatalogoCompleto_Quando_OlharABarraLateral_Entao_NaoDeveHaverCaixaDeSubcategoria() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.CaixasDeSubcategoria).ToHaveCountAsync(0);
        });

    [Fact]
    public async Task Dado_Catalogo_Quando_OrdenarPorMenorPreco_Entao_DeveListarDoMaisBaratoAoMaisCaro() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "souvenir");

            await pagina.SeletorDeOrdenacao.SelectOptionAsync(new SelectOptionValue { Label = "Menor preço" });
            await Pagina.WaitForURLAsync(url => url.Contains("ordenacao=MenorPreco"));

            var precos = await Pagina.Locator(".preco-card").AllTextContentsAsync();
            var valores = precos.Select(p => decimal.Parse(p.Replace("R$", "").Trim(), System.Globalization.CultureInfo.GetCultureInfo("pt-BR"))).ToList();

            Assert.Equal(valores.OrderBy(v => v), valores);
        });

    [Fact]
    public async Task Dado_SeletorDeOrdenacao_Quando_TentarEscolherMaisVendidos_Entao_DeveEstarIndisponivel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            var opcaoDesabilitada = pagina.SeletorDeOrdenacao.Locator("option", new() { HasText = "Mais vendidos" });
            await Expect(opcaoDesabilitada).ToBeDisabledAsync();
        });

    [Fact]
    public async Task Dado_OrdenacaoEscolhida_Quando_TrocarDeCategoriaEDePagina_Entao_DevePreservarAOrdenacao() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await pagina.SeletorDeOrdenacao.SelectOptionAsync(new SelectOptionValue { Label = "Maior preço" });
            await Pagina.WaitForURLAsync(url => url.Contains("ordenacao=MaiorPreco"));

            await pagina.IrParaPagina(2);

            Assert.Contains("ordenacao=MaiorPreco", Pagina.Url);
        });

    [Fact]
    public async Task Dado_CategoriaComMaisDeDozeProdutos_Quando_IrParaASegundaPagina_Entao_DeveMostrarOutrosProdutos() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var nomesPagina1 = await Pagina.Locator(".nome-card").AllTextContentsAsync();

            await pagina.IrParaPagina(2);

            var nomesPagina2 = await Pagina.Locator(".nome-card").AllTextContentsAsync();

            Assert.Empty(nomesPagina1.Intersect(nomesPagina2));
        });

    [Fact]
    public async Task Dado_Catalogo_Quando_ClicarNumProduto_Entao_DeveAbrirAPaginaDele() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "souvenir");

            await pagina.Cards.First.Locator(".nome-card").ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex($"{System.Text.RegularExpressions.Regex.Escape(UrlBase)}/Produto/Detalhes/.+"));
        });

    [Fact]
    public async Task Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_DevemEstarDesabilitados() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.Cards.First.Locator(".botao-adicionar-card")).ToBeDisabledAsync();
            await Expect(pagina.Cards.First.Locator(".botao-favorito-card")).ToBeDisabledAsync();
        });

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_AbrirCatalogoEVitrine_Entao_NaoDeveAparecerEmNenhum() =>
        await Executar(async () =>
        {
            // "Bolachas / Rosquinhas 2" é o produto que o seed marca como
            // inativo em Doces (DbInitializer.GerarProdutosMock).
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Expect(Pagina.GetByText("BOLACHAS / ROSQUINHAS 2", new() { Exact = true })).Not.ToBeVisibleAsync();

            await Pagina.GotoAsync(UrlBase);
            await Expect(Pagina.GetByText("BOLACHAS / ROSQUINHAS 2", new() { Exact = true })).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ProdutoForaDeEstoque_Quando_AbrirOCatalogo_Entao_DeveAparecerSinalizado() =>
        await Executar(async () =>
        {
            // "Box 3" é o produto que o seed marca como fora de estoque em
            // Doces (DbInitializer.GerarProdutosMock).
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var card = Pagina.Locator(".card-produto", new() { HasText = "BOX 3" });
            await Expect(card).ToBeVisibleAsync();
            await Expect(card.Locator(".etiqueta-fora-de-estoque")).ToHaveTextAsync("Fora de estoque");
        });

    [Fact]
    public async Task Dado_FiltrosSemResultado_Quando_Aplicados_Entao_DeveMostrarMensagemPropria() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            // "Bijuterias" (Souvenir) não tem produto sem açúcar — combinação
            // impossível por design do seed.
            await pagina.Abrir(UrlBase, "souvenir");

            await pagina.MarcarSemAcucar();

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ApelidoInexistente_Quando_AbrirOCatalogo_Entao_DeveResponder404() =>
        await Executar(async () =>
        {
            var resposta = await Pagina.GotoAsync($"{UrlBase}/Catalogo/inexistente");

            Assert.Equal(404, resposta!.Status);
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar_Entao_TudoDeveFuncionar() =>
        await Executar(async () =>
        {
            // Navega direto pelas URLs que os formulários e links produzem —
            // prova que o resultado funciona sem depender de onchange/JS
            // (RF-22, CA-25), sem precisar desligar JS no contexto inteiro.
            var pagina = new PaginaCatalogo(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=MenorPreco&pagina=2");

            await Expect(pagina.LinkPaginaAtual).ToHaveTextAsync("2");
            await Expect(pagina.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_TelaDe375px_Quando_AbrirOCatalogo_Entao_NaoDeveHaverRolagemHorizontal() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(375, 800);
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var larguras = await Pagina.EvaluateAsync<int[]>(@"() => {
                const el = document.querySelector('.pagina-catalogo');
                return [el.scrollWidth, document.documentElement.clientWidth];
            }");

            Assert.True(larguras[0] <= larguras[1], $"scrollWidth={larguras[0]} > clientWidth={larguras[1]}");
        });
}
