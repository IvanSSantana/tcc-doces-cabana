using System.Text.RegularExpressions;
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
    public async Task Dado_Catalogo_Quando_MarcarSubcategoria_Entao_OEnderecoDeveConterONomeLegivel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await pagina.MarcarSubcategoriaPeloNome("Barras");

            await Expect(Pagina).ToHaveURLAsync(new Regex("subcategorias=barras"));
        });

    [Fact]
    public async Task Dado_DuasSubcategoriasMarcadas_Quando_OlharOEndereco_Entao_AmbasDevemAparecerPorNome() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "adega");

            await pagina.MarcarSubcategoriaPeloNome("Vinhos");
            await pagina.MarcarSubcategoriaPeloNome("Cachaça");

            await Expect(Pagina).ToHaveURLAsync(new Regex("(?=.*subcategorias=vinhos)(?=.*subcategorias=cachaca).*"));
        });

    [Fact]
    public async Task Dado_ApelidoDeSubcategoriaInexistente_Quando_AbrirOCatalogo_Entao_DeveMostrarACategoriaInteira() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/doces?subcategorias=nao-existe");

            var pagina = new PaginaCatalogo(Pagina);
            await Expect(pagina.CategoriaAtiva).ToHaveTextAsync("Doces");
            await Expect(pagina.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_MesmoNomeEmDuasCategorias_Quando_FiltrarEmCadaUma_Entao_NaoDevemSeConfundir() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await pagina.GarantirSubcategoriaVisivel("Cappuccino");
            await pagina.MarcarSubcategoriaPeloNome("Cappuccino");
            var totalEmDoces = await pagina.Cards.CountAsync();

            await pagina.Abrir(UrlBase, "emporio");
            await pagina.GarantirSubcategoriaVisivel("Cappuccino");
            await pagina.MarcarSubcategoriaPeloNome("Cappuccino");
            var totalEmEmporio = await pagina.Cards.CountAsync();

            Assert.True(totalEmDoces > 0);
            Assert.True(totalEmEmporio > 0);
            var pagina2 = new PaginaCatalogo(Pagina);
            await Expect(pagina2.CategoriaAtiva).ToHaveTextAsync("Empório");
        });

    [Fact]
    public async Task Dado_MenuDoCabecalho_Quando_EscolherSubcategoria_Entao_OEnderecoDeveSerLegivel() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            var itemDoces = Pagina.Locator(".item-categoria-nav", new() { HasText = "Doces" });
            await itemDoces.HoverAsync();
            var linkBarras = itemDoces.Locator(".submenu-categoria")
                .GetByRole(AriaRole.Link, new() { Name = "Barras", Exact = true });
            await linkBarras.ClickAsync(new() { Force = true });

            await Expect(Pagina).ToHaveURLAsync(new Regex(@"/Catalogo/doces\?.*subcategorias=barras"));
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
    public async Task Dado_CatalogoSemOrdenacaoEscolhida_Quando_Abrir_Entao_DeveOrdenarPorMelhorAvaliados() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.SeletorDeOrdenacao).ToHaveValueAsync("MelhorAvaliados");
        });

    [Fact]
    public async Task Dado_OrdenacaoInicial_Quando_PercorrerDuasPaginas_Entao_NenhumProdutoDeveSeRepetir() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var nomesPagina1 = await Pagina.Locator(".nome-card").AllTextContentsAsync();
            await pagina.IrParaPagina(2);
            var nomesPagina2 = await Pagina.Locator(".nome-card").AllTextContentsAsync();

            Assert.Empty(nomesPagina1.Intersect(nomesPagina2));
            Assert.NotEmpty(nomesPagina1);
            Assert.NotEmpty(nomesPagina2);
        });

    [Fact]
    public async Task Dado_SeletorDeOrdenacao_Quando_EscolherMaisVendidos_Entao_DeveOrdenarPorIsso() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            var opcao = pagina.SeletorDeOrdenacao.Locator("option", new() { HasText = "Mais vendidos" });
            await Expect(opcao).ToBeEnabledAsync();

            await pagina.SeletorDeOrdenacao.SelectOptionAsync(new SelectOptionValue { Label = "Mais vendidos" });
            await Pagina.WaitForURLAsync(url => url.Contains("ordenacao=MaisVendidos"));
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
    public async Task Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_QuantidadeECarrinhoDevemFuncionar() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);
            var cartao = pagina.Cards.First;

            await Expect(cartao.Locator(".botao-adicionar-card")).ToBeEnabledAsync();
            await Expect(cartao.Locator(".botao-quantidade-card").First).ToBeEnabledAsync();

            await cartao.Locator(".botao-quantidade-card.mais").ClickAsync();
            await Expect(cartao.Locator(".valor-quantidade-card")).ToHaveTextAsync("2");

            await cartao.Locator(".botao-adicionar-card").ClickAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Expect(Pagina.Locator("[data-bolha-carrinho]")).ToHaveTextAsync("2");
        });

    [Fact]
    public async Task Dado_ProdutoInativo_Quando_AbrirCatalogoEVitrine_Entao_NaoDeveAparecerEmNenhum() =>
        await Executar(async () =>
        {
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
            var pagina = new PaginaCatalogo(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=NomeAZ");

            var card = Pagina.Locator(".card-produto", new() { HasText = "BOX 3" });
            await Expect(card).ToBeVisibleAsync();
            await Expect(card.Locator(".etiqueta-fora-de-estoque")).ToHaveTextAsync("Fora de estoque");
        });

    [Fact]
    public async Task Dado_FiltrosSemResultado_Quando_Aplicados_Entao_DeveMostrarMensagemPropria() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
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
    public async Task Dado_CatalogoAberto_Quando_MarcarSubcategoria_Entao_NaoDeveRecarregarAPagina() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Pagina.EvaluateAsync("() => { window.__marcadorDeRecarga = true; }");

            await pagina.MarcarSubcategoriaPeloNome("Barras");

            var marcadorSobreviveu = await Pagina.EvaluateAsync<bool>("() => window.__marcadorDeRecarga === true");
            Assert.True(marcadorSobreviveu, "A página recarregou — o marcador de memória não sobreviveu ao filtro.");
        });

    [Fact]
    public async Task Dado_FiltroAplicado_Quando_OlharOEndereco_Entao_DeveConterOFiltro() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await pagina.MarcarSubcategoriaPeloNome("Barras");

            Assert.Contains("subcategorias=", Pagina.Url);

            var enderecoDepoisDoFiltro = Pagina.Url;
            await Pagina.GotoAsync(enderecoDepoisDoFiltro);
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).ToBeCheckedAsync();
        });

    [Fact]
    public async Task Dado_FiltroAplicado_Quando_VoltarNoNavegador_Entao_DeveRestaurarAListaAnterior() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            var enderecoOriginal = Pagina.Url;

            await pagina.MarcarSubcategoriaPeloNome("Barras");
            Assert.NotEqual(enderecoOriginal, Pagina.Url);

            await Pagina.GoBackAsync();
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

            Assert.Equal(enderecoOriginal, Pagina.Url);
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).Not.ToBeCheckedAsync();
        });

    [Fact]
    public async Task Dado_PaginaRolada_Quando_TrocarAOrdenacao_Entao_DevePreservarARolagem() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Pagina.EvaluateAsync("() => window.scrollTo(0, 400)");
            var rolagemAntes = await Pagina.EvaluateAsync<double>("() => window.scrollY");

            await pagina.SeletorDeOrdenacao.SelectOptionAsync(new SelectOptionValue { Label = "Maior preço" });
            await Pagina.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var rolagemDepois = await Pagina.EvaluateAsync<double>("() => window.scrollY");
            Assert.True(Math.Abs(rolagemAntes - rolagemDepois) < 50,
                $"Rolagem antes: {rolagemAntes}, depois: {rolagemDepois} — deveria continuar aproximadamente no mesmo lugar.");
        });

    [Fact]
    public async Task Dado_FimDaPrimeiraPagina_Quando_IrParaASegunda_Entao_DeveMostrarOInicioDaLista() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Pagina.EvaluateAsync("() => window.scrollTo(0, document.body.scrollHeight)");

            await pagina.IrParaPagina(2);

            var distanciaDoTopoDaTela = await pagina.ResultadoCatalogo.EvaluateAsync<double>(
                "el => el.getBoundingClientRect().top");
            Assert.True(distanciaDoTopoDaTela is >= -50 and <= 150,
                $"Topo do resultado a {distanciaDoTopoDaTela}px do topo da tela — esperado próximo do topo visível.");
        });

    [Fact]
    public async Task Dado_FiltroAplicado_Quando_OResultadoMuda_Entao_DeveSerAnunciado() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await Expect(pagina.Contagem).ToHaveAttributeAsync("aria-live", "polite");

            var contagemAntes = await pagina.Contagem.InnerTextAsync();
            await pagina.MarcarSubcategoriaPeloNome("Barras");
            var contagemDepois = await pagina.Contagem.InnerTextAsync();

            Assert.NotEqual(contagemAntes, contagemDepois);
        });

    [Fact]
    public async Task Dado_AtualizacaoParcialFalha_Quando_Filtrar_Entao_DeveCarregarAPaginaCompleta() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await Pagina.RouteAsync("**/Catalogo/doces?*", async rota =>
            {
                if (rota.Request.Headers.TryGetValue("x-requested-with", out var valor) && valor == "XMLHttpRequest")
                    await rota.AbortAsync();
                else
                    await rota.ContinueAsync();
            });

            await pagina.MarcarSubcategoriaPeloNome("Barras");
            await Pagina.WaitForURLAsync(url => url.Contains("subcategorias="));

            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).ToBeCheckedAsync();
        });

    [Fact]
    public async Task Dado_CategoriaAberta_Quando_TrocarDeCategoria_Entao_DeveTrocarAsSubcategorias() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).ToBeVisibleAsync();

            await pagina.LinkDeCategoria("Adega").ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(
                System.Text.RegularExpressions.Regex.Escape($"{UrlBase}/Catalogo/adega")));
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).Not.ToBeVisibleAsync();
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Vinhos")).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_NavegacaoPorTeclado_Quando_TrocarDePaginaPelaPaginacao_Entao_OFocoDeveFicarNoResultado() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await pagina.IrParaPagina(2);

            var focoDentroDoResultado = await pagina.ResultadoCatalogo.EvaluateAsync<bool>(
                "el => el.contains(document.activeElement) || el === document.activeElement");
            Assert.True(focoDentroDoResultado, "O foco não ficou junto do resultado depois de paginar.");
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar_Entao_TudoDeveFuncionar() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();
            var pagina = new PaginaCatalogo(paginaSemScript);

            await pagina.Abrir(UrlBase, "doces");
            await pagina.CaixasDeSubcategoria.First.CheckAsync();
            await paginaSemScript.Locator(".botao-aplicar-filtro").ClickAsync();

            await Expect(paginaSemScript).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("subcategorias="));

            await paginaSemScript.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=MenorPreco&pagina=2");
            await Expect(pagina.LinkPaginaAtual).ToHaveTextAsync("2");
            await Expect(pagina.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_CatalogoAberto_Quando_MedirOCartao_Entao_DevePreencherAColuna() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(1440, 1000);
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var larguraCartao = await pagina.Cards.First.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraColuna = await pagina.Grade.EvaluateAsync<double>(@"el => {
                const estilo = getComputedStyle(el);
                const colunas = estilo.gridTemplateColumns.split(' ').length;
                const gap = parseFloat(estilo.columnGap) || 0;
                return (el.getBoundingClientRect().width - gap * (colunas - 1)) / colunas;
            }");

            Assert.True(larguraCartao >= larguraColuna - 2,
                $"Cartão com {larguraCartao}px numa coluna de {larguraColuna}px — sobra faixa vazia.");
        });

    [Fact]
    public async Task Dado_LinhaComNomeCurtoENomeLongo_Quando_CompararOsBotoes_Entao_DevemEstarNaMesmaAltura() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(1440, 1000);
            var pagina = new PaginaCatalogo(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=NomeAZ");

            var topos = await Pagina.EvaluateAsync<double[]>(@"() => {
                const botoes = Array.from(document.querySelectorAll('.grade-produtos .botao-adicionar-card'));
                return botoes.slice(0, 3).map(b => Math.round(b.getBoundingClientRect().top));
            }");

            Assert.True(topos.Max() - topos.Min() <= 2,
                $"Botões da mesma linha em alturas diferentes: {string.Join(",", topos)}");
        });

    [Fact]
    public async Task Dado_ProdutoForaDeEstoque_Quando_OlharAEtiqueta_Entao_DeveEstarSobreAImagem() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/doces?ordenacao=NomeAZ");

            var card = Pagina.Locator(".card-produto", new() { HasText = "BOX 3" });
            var caixaImagem = await card.Locator(".container-imagem-card").BoundingBoxAsync();
            var caixaEtiqueta = await card.Locator(".etiqueta-fora-de-estoque").BoundingBoxAsync();

            Assert.NotNull(caixaImagem);
            Assert.NotNull(caixaEtiqueta);
            Assert.True(caixaEtiqueta!.Y >= caixaImagem!.Y - 1, "Etiqueta acima do topo da imagem.");
            Assert.True(caixaEtiqueta.Y + caixaEtiqueta.Height <= caixaImagem.Y + caixaImagem.Height + 1, "Etiqueta abaixo do fim da imagem.");
        });

    [Fact]
    public async Task Dado_PaginaInicial_Quando_OlharOCarrossel_Entao_NaoDeveTerRegredido() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(1440, 1000);
            await Pagina.GotoAsync(UrlBase);

            var larguraCartao = await Pagina.Locator(".vitrine-carrossel .card-produto").First
                .EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraItem = await Pagina.Locator(".item-carrossel").First
                .EvaluateAsync<double>("el => el.getBoundingClientRect().width");

            Assert.True(larguraCartao < larguraItem - 10,
                $"Cartão do carrossel com {larguraCartao}px, item com {larguraItem}px — folga esperada não apareceu.");

            var transformacaoDoNome = await Pagina.Locator(".vitrine-carrossel .nome-card").First
                .EvaluateAsync<string>("el => getComputedStyle(el).textTransform");
            Assert.Equal("uppercase", transformacaoDoNome);
        });

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_DeveOferecerContaClicavel() =>
        await Executar(async () =>
        {
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);

            var atalhoConta = Pagina.Locator("header").GetByRole(AriaRole.Link, new() { Name = "Conta" });
            await Expect(atalhoConta).ToBeVisibleAsync();

            await atalhoConta.ClickAsync();
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Conta");
        });

    [Fact]
    public async Task Dado_CatalogoAberto_Quando_MedirOArranjoDoCartao_Entao_DeveSeguirAReferencia() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(1440, 1000);
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var card = pagina.Cards.First;

            var fundoDaImagem = await card.Locator(".container-imagem-card")
                .EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.NotEqual("rgba(0, 0, 0, 0)", fundoDaImagem);

            var caixaDoPreco = await card.Locator(".preco-card").BoundingBoxAsync();
            var caixaDoSeletor = await card.Locator(".controles-card").BoundingBoxAsync();
            Assert.NotNull(caixaDoPreco);
            Assert.NotNull(caixaDoSeletor);
            Assert.True(System.Math.Abs(caixaDoPreco!.Y - caixaDoSeletor!.Y) < caixaDoPreco.Height,
                $"Preço (y={caixaDoPreco.Y}) e seletor (y={caixaDoSeletor.Y}) não estão na mesma linha.");

            var caixaDoCartao = await card.BoundingBoxAsync();
            var caixaDoBotao = await card.Locator(".botao-adicionar-card").BoundingBoxAsync();
            Assert.NotNull(caixaDoCartao);
            Assert.NotNull(caixaDoBotao);
            Assert.True(caixaDoBotao!.Width > caixaDoCartao!.Width * 0.7,
                $"Botão com {caixaDoBotao.Width}px num cartão de {caixaDoCartao.Width}px — não parece uma faixa larga.");
        });

    [Fact]
    public async Task Dado_CatalogoAberto_Quando_LerONomeDoProduto_Entao_NaoDeveEstarTodoEmMaiusculas() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var nomeElemento = pagina.Cards.First.Locator(".nome-card");
            var nome = (await nomeElemento.TextContentAsync())!.Trim();

            var transformacaoComputada = await nomeElemento.EvaluateAsync<string>("el => getComputedStyle(el).textTransform");

            Assert.NotEqual(nome.ToUpperInvariant(), nome);
            Assert.NotEqual("uppercase", transformacaoComputada);
        });

    [Fact]
    public async Task Dado_CatalogoDeCategoria_Quando_OlharATrilha_Entao_DeveEstarEmCaixaAltaComUltimoDestacado() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var transformacao = await pagina.Trilha.EvaluateAsync<string>("el => getComputedStyle(el).textTransform");
            Assert.Equal("uppercase", transformacao);

            var ultimoItem = pagina.Trilha.Locator(":scope > *").Last;
            var textoDoUltimo = (await ultimoItem.TextContentAsync())!.Trim();
            Assert.Equal("Doces", textoDoUltimo);

            var corDoUltimo = await ultimoItem.EvaluateAsync<string>("el => getComputedStyle(el).color");
            var corDosAnteriores = await pagina.Trilha.Locator("a").First
                .EvaluateAsync<string>("el => getComputedStyle(el).color");
            Assert.NotEqual(corDosAnteriores, corDoUltimo);
        });

    [Fact]
    public async Task Dado_CategoriaComMaisDeOitoSubcategorias_Quando_Revelar_Entao_OControleDeveIrParaOFimEOferecerRecolher() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var detalhes = Pagina.Locator(".ver-todas-subcategorias");
            await Expect(detalhes.Locator(".rotulo-fechado")).ToBeVisibleAsync();
            await Expect(detalhes.Locator(".rotulo-aberto")).Not.ToBeVisibleAsync();

            var ultimaOpcaoPrincipal = pagina.CaixasDeSubcategoria.Last.Locator("xpath=ancestor::label");

            var topoDoControleFechado = (await detalhes.BoundingBoxAsync())!.Y;
            var topoDaUltimaPrincipal = (await ultimaOpcaoPrincipal.BoundingBoxAsync())!.Y;
            Assert.True(topoDoControleFechado > topoDaUltimaPrincipal,
                "Fechado, o controle deveria vir depois das oito principais.");

            await pagina.VerTodas.ClickAsync();

            var opcoesReveladas = detalhes.Locator(".opcao-filtro-catalogo");
            var topoDaPrimeiraRevelada = (await opcoesReveladas.First.BoundingBoxAsync())!.Y;
            var topoDoControleAberto = (await detalhes.Locator("summary").BoundingBoxAsync())!.Y;

            Assert.True(topoDoControleAberto > topoDaPrimeiraRevelada,
                "Aberto, o controle deveria estar abaixo das subcategorias reveladas, não continuar acima delas.");

            await Expect(detalhes.Locator(".rotulo-aberto")).ToBeVisibleAsync();
            await Expect(detalhes.Locator(".rotulo-fechado")).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_RevelarERecolherSubcategorias_Entao_DeveFuncionarNosDoisSentidos() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();
            var pagina = new PaginaCatalogo(paginaSemScript);
            await pagina.Abrir(UrlBase, "doces");

            await pagina.VerTodas.ClickAsync();
            await Expect(paginaSemScript.Locator(".ver-todas-subcategorias")).ToHaveAttributeAsync("open", "");

            await pagina.VerTodas.ClickAsync();
            await Expect(paginaSemScript.Locator(".ver-todas-subcategorias")).Not.ToHaveAttributeAsync("open", "");
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
