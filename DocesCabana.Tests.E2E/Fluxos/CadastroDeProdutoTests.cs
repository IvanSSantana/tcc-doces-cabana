using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CadastroDeProdutoTests : TesteE2E
{
    public CadastroDeProdutoTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar() =>
        await Executar(async () =>
        {
            await EntrarComoAdministrador();

            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);
            await paginaProduto.Preencher(
                "Brigadeiro Gourmet E2E", 9.90m, "https://exemplo.com/imagens/brigadeiro.jpg");
            await paginaProduto.Enviar();

            await Expect(paginaProduto.MensagemDeConfirmacao).ToHaveTextAsync("Produto cadastrado com sucesso!");
        });

    [Fact]
    public async Task Dado_PrecoInvalido_Quando_CadastrarProduto_Entao_DeveMostrarErroNoCampoSemCadastrar() =>
        await Executar(async () =>
        {
            await EntrarComoAdministrador();

            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);
            await paginaProduto.Preencher(
                "Produto Preço Inválido", 0m, "https://exemplo.com/imagens/invalido.jpg");
            await paginaProduto.Enviar();

            await Expect(paginaProduto.ErroDePreco).ToHaveTextAsync("Preço deve ser maior que zero.");
            await Expect(paginaProduto.MensagemDeConfirmacao).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Administrador_Quando_AbrirOCadastroDeProduto_Entao_DeveTerTituloEContencao() =>
        await Executar(async () =>
        {
            // CA-17 (spec 016): mesmo padrão do cadastro de administrador —
            // título, largura contida, campos no desenho da marca.
            await EntrarComoAdministrador();

            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);

            var container = Pagina.Locator(".container-autenticacao");
            await Expect(container).ToBeVisibleAsync();
            await Expect(container.Locator(".titulo-tela")).ToHaveTextAsync("Cadastrar Produto");

            var larguraDoContainer = await container.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            Assert.True(larguraDoContainer <= 680, $"Container esperado com no máximo 680px, mediu {larguraDoContainer}px.");
        });

    [Fact]
    public async Task Dado_TelaEstreita_Quando_AbrirOCadastroDeProduto_Entao_OFormularioNaoDeveTransbordar() =>
        await Executar(async () =>
        {
            // Escopo é o formulário desta feature, não a página inteira: o
            // cabeçalho (via _Layout) já estoura horizontalmente a 375px
            // desde a spec 009, registrado como pré-existente e fora de
            // escopo em toda feature desde então (013 §10, 015 checklist) —
            // não é este formulário que o causa, e corrigi-lo aqui seria
            // escopo que ninguém pediu.
            await EntrarComoAdministrador();

            await Pagina.SetViewportSizeAsync(375, 900);
            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);

            var container = Pagina.Locator(".container-autenticacao");
            var larguraDoContainer = await container.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraDaTela = await Pagina.EvaluateAsync<double>("() => document.documentElement.clientWidth");

            Assert.True(larguraDoContainer <= larguraDaTela,
                $"Container do formulário ({larguraDoContainer}px) excede a tela ({larguraDaTela}px).");

            // Largura de conteúdo do container: sua própria largura menos o
            // padding horizontal — não a largura do container em si, que
            // inclui esse padding (root font-size cai para 14px abaixo de
            // 768px, então 1.5rem de padding vale 21px aqui, não 24px).
            var larguraDeConteudo = await container.EvaluateAsync<double>(@"el => {
                const estilo = getComputedStyle(el);
                return el.getBoundingClientRect().width
                    - parseFloat(estilo.paddingLeft) - parseFloat(estilo.paddingRight);
            }");

            // Os pares em linha dupla empilham (RF-17): o campo de Preço
            // (dentro do primeiro .linha-dupla) ocupa a largura de
            // conteúdo inteira, não a metade dela.
            var larguraDoCampoPreco = await container.Locator(".linha-dupla .campo-texto").First.EvaluateAsync<double>(
                "el => el.getBoundingClientRect().width");
            Assert.True(larguraDoCampoPreco >= larguraDeConteudo * 0.95,
                $"Campo esperado ocupando a largura de conteúdo ({larguraDeConteudo}px), mediu {larguraDoCampoPreco}px.");
        });

    private async Task EntrarComoAdministrador()
    {
        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
        await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");
    }
}
