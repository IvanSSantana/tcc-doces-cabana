using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

/// <summary>
/// Prova o que unidade e integração não alcançam: o código guardado na sessão sobrevive
/// entre requisições e passos do fechamento, e as views mostram o desconto em cada um.
/// O passo de Pagamento não oferece o formulário no E2E porque a cotação de frete real
/// não responde sem credencial (pendência da 022); o resumo do passo, sim.
/// </summary>
public class CupomTests : TesteE2E
{
    public CupomTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_ClienteLogado_Quando_AplicarBemVindo10_Entao_DescontoDeveAparecerNoResumoENoPassoDePagamento() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();

            var paginaCatalogo = new PaginaCatalogo(Pagina);
            await paginaCatalogo.Abrir(UrlBase, "doces");
            var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

            var carrinho = new PaginaCarrinho(Pagina);
            await carrinho.SemearItem(UrlBase, produtoId, 2);
            await carrinho.Abrir(UrlBase);

            await carrinho.AplicarCupom("bemvindo10");

            await Expect(carrinho.CupomAplicado).ToContainTextAsync("BEMVINDO10");
            await Expect(carrinho.CupomAplicado).ToContainTextAsync("10%");
            await Expect(carrinho.LinhaDescontoDoCupom).ToContainTextAsync("BEMVINDO10");

            await Pagina.GotoAsync($"{UrlBase}/Carrinho?passo=Pagamento");

            var fechamento = new PaginaFechamento(Pagina);
            await Expect(fechamento.PassoAtivo).ToHaveTextAsync("Pagamento");
            await Expect(carrinho.CupomAplicado).ToContainTextAsync("BEMVINDO10");
            await Expect(carrinho.LinhaDescontoDoCupom).ToContainTextAsync("BEMVINDO10");
        });

    private async Task CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("cupom");
        var dados = new DadosDeCadastro(
            "Cliente Cupom E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), "SenhaForte@2026");

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, "SenhaForte@2026");
    }
}
