using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CarrinhoTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public CarrinhoTests(FixtureE2E fixture) : base(fixture) { }

    private async Task<string> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("carrinho");
        var dados = new DadosDeCadastro(
            "Cliente Carrinho E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, SenhaValida);

        return email;
    }

    private async Task<Guid> ObterProdutoAtivo(int indice = 0)
    {
        var pagina = new PaginaCatalogo(Pagina);
        await pagina.Abrir(UrlBase, "doces");
        var texto = await pagina.Cards.Nth(indice).GetAttributeAsync("data-produto-id");
        return Guid.Parse(texto!);
    }

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_AcrescentarAlterarERemover_Entao_OCarrinhoDeveAcompanhar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var paginaCatalogo = new PaginaCatalogo(Pagina);
            await paginaCatalogo.Abrir(UrlBase, "doces");
            var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

            await paginaCatalogo.Cards.First.Locator(".botao-adicionar-card").ClickAsync();
            await Pagina.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

            var pagina = new PaginaCarrinho(Pagina);
            await pagina.Abrir(UrlBase);
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();

            await pagina.AumentarQuantidade(produtoId);
            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");

            await pagina.Remover(produtoId);
            await Expect(pagina.ItemPeloProduto(produtoId)).ToHaveCountAsync(0);
            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CarrinhosNosDoisLados_Quando_Entrar_Entao_AsQuantidadesDevemSomar() =>
        await Executar(async () =>
        {
            var email = await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 3);

            await Sair();
            await Pagina.GotoAsync(UrlBase);

            await pagina.SemearItem(UrlBase, produtoId, 2);

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);

            await pagina.Abrir(UrlBase);

            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("5");
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();
}
