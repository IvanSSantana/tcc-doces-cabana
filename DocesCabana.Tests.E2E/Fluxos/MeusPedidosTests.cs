using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class MeusPedidosTests : TesteE2E
{
    public MeusPedidosTests(FixtureE2E fixture) : base(fixture) { }

    private async Task Entrar(string email, string senha)
    {
        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Abrir(UrlBase);
        await paginaLogin.Entrar(email, senha);
    }

    [Fact]
    public async Task Dado_UmPedido_Quando_Abrir_Entao_DeveLevarAoDetalheComItensEValores() =>
        await Executar(async () =>
        {
            await Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaMeusPedidos(Pagina);
            await pagina.Abrir(UrlBase);
            await pagina.AbrirPrimeiroPedido();

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Pedido/Detalhe/"));
            await Expect(Pagina.Locator(".item-detalhe-pedido").First).ToBeVisibleAsync();
            await Expect(Pagina.Locator(".total-detalhe-pedido")).ToBeVisibleAsync();
            await Expect(Pagina.Locator(".bloco-detalhe-pedido")).ToHaveCountAsync(2);
        });

}
