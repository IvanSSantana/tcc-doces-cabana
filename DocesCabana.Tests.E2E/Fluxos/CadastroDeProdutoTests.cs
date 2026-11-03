using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CadastroDeProdutoTests : TesteE2E
{
    public CadastroDeProdutoTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    [Trait("Categoria", "Externo")]
    public async Task Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar() =>
        await Executar(async () =>
        {
            await EntrarComoAdministrador();

            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);
            await paginaProduto.Preencher("Brigadeiro Gourmet E2E", 9.90m);
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
            await paginaProduto.Preencher("Produto Preço Inválido", 0m);
            await paginaProduto.Enviar();

            await Expect(paginaProduto.ErroDePreco).ToHaveTextAsync("Preço deve ser maior que zero.");
            await Expect(paginaProduto.MensagemDeConfirmacao).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_SemCredencialDeArmazenamento_Quando_CadastrarProduto_Entao_DeveRecusarComMensagemEspecificaSemCadastrar() =>
        await Executar(async () =>
        {
            await EntrarComoAdministrador();

            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);
            await paginaProduto.Preencher("Produto Sem Credencial E2E", 12.90m);
            await paginaProduto.Enviar();

            await Expect(paginaProduto.ErroGeral).ToHaveTextAsync("Armazenamento de imagem não configurado.");
            await Expect(paginaProduto.MensagemDeConfirmacao).Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Administrador_Quando_AbrirOCadastroDeProduto_Entao_DeveTerTituloEContencao() =>
        await Executar(async () =>
        {
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
            await EntrarComoAdministrador();

            await Pagina.SetViewportSizeAsync(375, 900);
            var paginaProduto = new PaginaCadastroProduto(Pagina);
            await paginaProduto.Abrir(UrlBase);

            var container = Pagina.Locator(".container-autenticacao");
            var larguraDoContainer = await container.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraDaTela = await Pagina.EvaluateAsync<double>("() => document.documentElement.clientWidth");

            Assert.True(larguraDoContainer <= larguraDaTela,
                $"Container do formulário ({larguraDoContainer}px) excede a tela ({larguraDaTela}px).");

            var larguraDeConteudo = await container.EvaluateAsync<double>(@"el => {
                const estilo = getComputedStyle(el);
                return el.getBoundingClientRect().width
                    - parseFloat(estilo.paddingLeft) - parseFloat(estilo.paddingRight);
            }");

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
