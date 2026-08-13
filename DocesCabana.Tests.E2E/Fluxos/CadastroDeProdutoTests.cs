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

    private async Task EntrarComoAdministrador()
    {
        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
        await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");
    }
}
