using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class FreteTests : TesteE2E
{
    public FreteTests(FixtureE2E fixture) : base(fixture) { }

    private async Task<Guid> ObterProdutoAtivo(int indice = 0)
    {
        var pagina = new PaginaCatalogo(Pagina);
        await pagina.Abrir(UrlBase, "doces");
        var texto = await pagina.Cards.Nth(indice).GetAttributeAsync("data-produto-id");
        return Guid.Parse(texto!);
    }

    [Fact]
    public async Task Dado_CepComFormatoInvalido_Quando_Calcular_Entao_DeveMostrarErroNoCampoSemDerrubarOCarrinho() =>
        await Executar(async () =>
        {
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.CalcularFrete("123");

            await Expect(pagina.ErroCep).ToBeVisibleAsync();
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CarrinhoVazio_Quando_AbrirATela_Entao_NaoDeveOferecerOCalculoDeFrete() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
            await Expect(pagina.FormularioFrete).ToHaveCountAsync(0);
        });

    [Fact]
    public async Task Dado_ServicoDeEntregaIndisponivel_Quando_Calcular_Entao_DeveAvisarSemDerrubarOCarrinho() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            var paginaCatalogo = new PaginaCatalogo(paginaSemScript);
            await paginaCatalogo.Abrir(UrlBase, "doces");
            var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);
            await paginaCatalogo.Cards.First.Locator(".botao-adicionar-card").ClickAsync();

            var pagina = new PaginaCarrinho(paginaSemScript);
            await pagina.CalcularFrete("01310000");

            await Expect(pagina.MensagemFalhaFrete).ToBeVisibleAsync();
            await Expect(pagina.OpcoesDeFrete).ToHaveCountAsync(0);
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    private async Task<string> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("frete");
        var dados = new DadosDeCadastro(
            "Cliente Frete E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), "SenhaForte@2026");

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, "SenhaForte@2026");

        return email;
    }
}
