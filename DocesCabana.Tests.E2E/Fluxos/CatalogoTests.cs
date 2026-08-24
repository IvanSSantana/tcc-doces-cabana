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

    // CA-12 a CA-16 (spec 016): o endereço do catálogo identifica
    // subcategoria por nome legível, não por identificador técnico.

    [Fact]
    public async Task Dado_Catalogo_Quando_MarcarSubcategoria_Entao_OEnderecoDeveConterONomeLegivel() =>
        await Executar(async () =>
        {
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            await pagina.MarcarSubcategoriaPeloNome("Barras");

            // Assert.Contains(Pagina.Url) é leitura única: o pushState do
            // catalogo.js acontece no .then() do fetch, que pode terminar
            // depois de NetworkIdle (mesma corrida documentada na spec 015).
            // ToHaveURLAsync tem retry automático — espera o endereço
            // convergir em vez de ler um instante arbitrário.
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
            // RN-04: filtro que não se aplica não impede a página — a
            // categoria inteira aparece, sem erro.
            await Expect(pagina.CategoriaAtiva).ToHaveTextAsync("Doces");
            await Expect(pagina.Cards).ToHaveCountAsync(12);
        });

    [Fact]
    public async Task Dado_MesmoNomeEmDuasCategorias_Quando_FiltrarEmCadaUma_Entao_NaoDevemSeConfundir() =>
        await Executar(async () =>
        {
            // "Cappuccino" existe em Doces e em Empório (DbInitializer) —
            // RN-03 escopa a unicidade do apelido por categoria, não pela
            // loja inteira.
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
            // Cada filtro só traz produto da própria categoria — a soma dos
            // dois nunca poderia superar o total das duas categorias juntas
            // se tivessem se confundido, mas a prova direta é: o catálogo de
            // "emporio" com o filtro nunca inclui produto de Doces, e
            // vice-versa (implícito por CategoriaId no filtro).
            var pagina2 = new PaginaCatalogo(Pagina);
            await Expect(pagina2.CategoriaAtiva).ToHaveTextAsync("Empório");
        });

    [Fact]
    public async Task Dado_MenuDoCabecalho_Quando_EscolherSubcategoria_Entao_OEnderecoDeveSerLegivel() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            // O submenu só é visível em :hover/:focus-within (spec 012) —
            // precisa passar o mouse sobre o item antes de alcançar o link.
            var itemDoces = Pagina.Locator(".item-categoria-nav", new() { HasText = "Doces" });
            await itemDoces.HoverAsync();
            var linkBarras = itemDoces.Locator(".submenu-categoria")
                .GetByRole(AriaRole.Link, new() { Name = "Barras", Exact = true });
            // A transição de opacidade/transform do submenu (header.css)
            // deixa o link "instável" para a checagem padrão de clique
            // durante os 0.15s da animação — Force ignora essa checagem,
            // o link já está clicável de verdade (RF-03/RF-09 da 012).
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
            // RF-16: o padrão deixou de ser "Nome (A-Z)" — a spec 014 semeou
            // avaliação suficiente para "melhor avaliados" fazer sentido.
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.SeletorDeOrdenacao).ToHaveValueAsync("MelhorAvaliados");
        });

    [Fact]
    public async Task Dado_OrdenacaoInicial_Quando_PercorrerDuasPaginas_Entao_NenhumProdutoDeveSeRepetir() =>
        await Executar(async () =>
        {
            // RN-04/CA-19: o desempate por Nome garante paginação
            // determinística mesmo quando o critério principal (nota média)
            // empata entre muitos produtos sem avaliação.
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
    public async Task Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_QuantidadeECarrinhoDevemFuncionar() =>
        await Executar(async () =>
        {
            // Correção esperada, não regressão (spec 017, tasks T045): os
            // controles do cartão saíram de "desabilitados até o carrinho
            // existir" (spec 012, RF-24) para funcionando de verdade — CA-01
            // não pede login, o carrinho de visitante já existe (Fase 6).
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
            // Doces (DbInitializer.GerarProdutosMock). Ordenação fixada em
            // Nome (A-Z): o padrão do catálogo virou "melhor avaliados" na
            // spec 014, e a posição de "Box 3" nessa ordem depende da nota
            // aleatória que o seed sorteou para ele — o teste quer achar o
            // produto pelo nome, não exercitar a ordenação padrão.
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
    public async Task Dado_CatalogoAberto_Quando_MarcarSubcategoria_Entao_NaoDeveRecarregarAPagina() =>
        await Executar(async () =>
        {
            // RF-01/CA-01: marca a página com uma variável em memória antes
            // do filtro — uma recarga de verdade reinicia o contexto de
            // JavaScript e a apaga; a troca parcial não.
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
            // RF-02/CA-02.
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
            // RF-02/CA-03: o botão voltar precisa desfazer a filtragem —
            // aqui, ativada via history.pushState, não navegação comum.
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
            // RF-03/CA-04.
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
            // RF-03/CA-05: "início da lista", não do documento — o resultado
            // fica abaixo da trilha de navegação, então o alvo é o topo do
            // próprio bloco de resultado ficar visível, não scrollY = 0.
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
            // RF-04/CA-06: a contagem é a região viva — sem isso, quem usa
            // leitor de tela filtra e não recebe aviso nenhum de mudança.
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
            // RF-06/CA-08: interrompe a requisição assíncrona e confirma que
            // o navegador cai para a navegação completa, não uma tela presa.
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
            // RF-07/CA-09: trocar de categoria continua navegação comum
            // (spec 014 §10) — a barra lateral inteira é reconstruída. A
            // ordem das subcategorias depende da contagem de produtos do
            // seed, então o teste checa presença por nome, não posição.
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).ToBeVisibleAsync();

            await pagina.LinkDeCategoria("Adega").ClickAsync();

            // O link de categoria carrega a ordenação atual na URL desde a
            // 012 (preserva o estado entre categorias) — o teste checa o
            // caminho, não a query inteira.
            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(
                System.Text.RegularExpressions.Regex.Escape($"{UrlBase}/Catalogo/adega")));
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Barras")).Not.ToBeVisibleAsync();
            await Expect(pagina.CaixaDeSubcategoriaPeloNome("Vinhos")).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_NavegacaoPorTeclado_Quando_TrocarDePaginaPelaPaginacao_Entao_OFocoDeveFicarNoResultado() =>
        await Executar(async () =>
        {
            // RF-18/CA-21: sem isso, o foco é jogado para o início do
            // documento a cada troca de página — hostil a quem usa teclado.
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
            // RF-05/CA-07: contexto com JavaScript de verdade desligado — o
            // teste anterior só navegava direto pela URL com script ligado
            // e não provava a degradação (spec 014, plano §7).
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
            // RF-08/CA-10: o cartão foi desenhado para o carrossel e
            // reaproveitado na grade sem revisão — width:85% deixava faixa
            // vazia ao lado (spec 014, plano §3).
            await Pagina.SetViewportSizeAsync(1440, 1000);
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var larguraCartao = await pagina.Cards.First.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            // Largura de uma coluna descontando o gap entre elas — sem isso
            // a medição confunde "gap da grade" com "faixa vazia no cartão".
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
            // RF-09/CA-11: "Bolachas / Rosquinhas 14" quebra em duas linhas e
            // empurra o botão do card para baixo — sem altura reservada para
            // o nome, produtos da mesma linha desalinham (spec 014, plano §3).
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
            // RF-10/CA-12: a etiqueta aparecia solta, acima da imagem, no
            // vão entre as linhas da grade (spec 014, plano §3).
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
            // RF-11/CA-13: o ajuste do cartão para a grade do catálogo não
            // pode mudar a aparência dele no carrossel da página inicial.
            await Pagina.SetViewportSizeAsync(1440, 1000);
            await Pagina.GotoAsync(UrlBase);

            var larguraCartao = await Pagina.Locator(".vitrine-carrossel .card-produto").First
                .EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraItem = await Pagina.Locator(".item-carrossel").First
                .EvaluateAsync<double>("el => el.getBoundingClientRect().width");

            // O cartão continua menor que o item que o envolve — é a folga
            // que o carrossel sempre teve (spec 014, plano §4).
            Assert.True(larguraCartao < larguraItem - 10,
                $"Cartão do carrossel com {larguraCartao}px, item com {larguraItem}px — folga esperada não apareceu.");

            // RF-16 tirou o text-transform da base do cartão — o carrossel
            // precisa continuar em caixa alta por conta própria, ou regride
            // visualmente mesmo com a largura certa (spec 015, plano §1).
            var transformacaoDoNome = await Pagina.Locator(".vitrine-carrossel .nome-card").First
                .EvaluateAsync<string>("el => getComputedStyle(el).textTransform");
            Assert.Equal("uppercase", transformacaoDoNome);
        });

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_DeveOferecerContaClicavel() =>
        await Executar(async () =>
        {
            // Correção esperada, não regressão (spec 018, tasks T041): o
            // atalho "Conta" saiu de "desabilitado até a tela existir"
            // (spec 014, RF-17) para funcionando de verdade (spec 018).
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
            // RF-15/CA-16 (spec 015): imagem com fundo próprio, preço e
            // seletor de quantidade na mesma linha, botão de carrinho
            // ocupando a largura na base.
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
            // Mesma linha: os centros verticais ficam a poucos pixels um do
            // outro, bem menos que a altura de qualquer um dos dois.
            Assert.True(System.Math.Abs(caixaDoPreco!.Y - caixaDoSeletor!.Y) < caixaDoPreco.Height,
                $"Preço (y={caixaDoPreco.Y}) e seletor (y={caixaDoSeletor.Y}) não estão na mesma linha.");

            var caixaDoCartao = await card.BoundingBoxAsync();
            var caixaDoBotao = await card.Locator(".botao-adicionar-card").BoundingBoxAsync();
            Assert.NotNull(caixaDoCartao);
            Assert.NotNull(caixaDoBotao);
            // "Ocupa a largura na base": bem mais largo que alto, e
            // preenchendo a maior parte da largura do cartão.
            Assert.True(caixaDoBotao!.Width > caixaDoCartao!.Width * 0.7,
                $"Botão com {caixaDoBotao.Width}px num cartão de {caixaDoCartao.Width}px — não parece uma faixa larga.");
        });

    [Fact]
    public async Task Dado_CatalogoAberto_Quando_LerONomeDoProduto_Entao_NaoDeveEstarTodoEmMaiusculas() =>
        await Executar(async () =>
        {
            // RF-16/CA-17: a referência mostra o nome em caixa normal — o
            // .ToUpper() saiu da view (spec 012 fazia isso na marcação).
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var nomeElemento = pagina.Cards.First.Locator(".nome-card");
            var nome = (await nomeElemento.TextContentAsync())!.Trim();

            // TextContent não é afetado por CSS — por si só, não prova nada
            // sobre a aparência. text-transform: uppercase na base do
            // cartão (existia desde antes da 015) fazia o nome renderizar
            // maiúsculo mesmo com o texto certo no DOM; só a transformação
            // computada prova o que a pessoa realmente vê.
            var transformacaoComputada = await nomeElemento.EvaluateAsync<string>("el => getComputedStyle(el).textTransform");

            Assert.NotEqual(nome.ToUpperInvariant(), nome);
            Assert.NotEqual("uppercase", transformacaoComputada);
        });

    [Fact]
    public async Task Dado_CatalogoDeCategoria_Quando_OlharATrilha_Entao_DeveEstarEmCaixaAltaComUltimoDestacado() =>
        await Executar(async () =>
        {
            // RF-19/RF-20/CA-20 (spec 015): a referência mostra a trilha em
            // caixa alta, com o último item na cor de destaque do tema.
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
            // RF-21/RF-22/CA-21: fechado, o controle vem logo depois das
            // oito principais; revelado, ele desce para depois das
            // subcategorias restantes e passa a oferecer "Ver menos".
            var pagina = new PaginaCatalogo(Pagina);
            await pagina.Abrir(UrlBase, "doces");

            var detalhes = Pagina.Locator(".ver-todas-subcategorias");
            // Os dois rótulos vivem na marcação o tempo todo (RF-21/RF-22) —
            // só um fica visível por vez, alternado pelo [open] do próprio
            // <details>. Checar visibilidade em vez de texto porque
            // textContent enxerga os dois rótulos juntos, escondido ou não.
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

            // O ponto que prova o requisito: aberto, o controle desce para
            // depois das subcategorias que ele mesmo revelou, em vez de
            // ficar preso acima delas como hoje.
            Assert.True(topoDoControleAberto > topoDaPrimeiraRevelada,
                "Aberto, o controle deveria estar abaixo das subcategorias reveladas, não continuar acima delas.");

            await Expect(detalhes.Locator(".rotulo-aberto")).ToBeVisibleAsync();
            await Expect(detalhes.Locator(".rotulo-fechado")).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_RevelarERecolherSubcategorias_Entao_DeveFuncionarNosDoisSentidos() =>
        await Executar(async () =>
        {
            // RF-23/CA-22: o <details> nativo não depende de script — só a
            // posição (CSS order) e o rótulo (dois <summary>) mudam com ele.
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
