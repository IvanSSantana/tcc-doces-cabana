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
    public async Task Dado_ClienteAutenticado_Quando_AbrirOMenuDaConta_Entao_OAtalhoDePedidosDeveFuncionar() =>
        await Executar(async () =>
        {
            // CA-01
            await Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaMeusPedidos(Pagina);
            await Pagina.GotoAsync($"{UrlBase}/Conta");
            await pagina.LinkMeusPedidosNoMenu.ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Pedido/Meus");
        });

    [Fact]
    public async Task Dado_ClienteComPedidos_Quando_AbrirALista_Entao_DeveMostrarOsPedidosComOMaisRecentePrimeiro() =>
        await Executar(async () =>
        {
            // CA-02/CA-03: o cliente semeado (cliente1) tem dois pedidos,
            // com situações diferentes e o mais recente ("Entregue")
            // gravado depois do mais antigo ("Confirmado" — DbInitializer).
            await Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaMeusPedidos(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.Cartoes).ToHaveCountAsync(2);
            await Expect(pagina.Cartoes.First.Locator(".situacao-cartao-pedido")).ToHaveTextAsync("Entregue");
        });

    [Fact]
    public async Task Dado_UmPedido_Quando_Abrir_Entao_DeveLevarAoDetalheComItensEValores() =>
        await Executar(async () =>
        {
            // CA-05
            await Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaMeusPedidos(Pagina);
            await pagina.Abrir(UrlBase);
            await pagina.AbrirPrimeiroPedido();

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Pedido/Detalhe/"));
            await Expect(Pagina.Locator(".item-detalhe-pedido").First).ToBeVisibleAsync();
            await Expect(Pagina.Locator(".total-detalhe-pedido")).ToBeVisibleAsync();
            await Expect(Pagina.Locator(".bloco-detalhe-pedido")).ToHaveCountAsync(2); // entrega + pagamento
        });

    [Fact]
    public async Task Dado_Visitante_Quando_TentarAbrirALista_Entao_DeveSerLevadoAEntrar() =>
        await Executar(async () =>
        {
            // CA-09
            var pagina = new PaginaMeusPedidos(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Autenticacao/Login"));
        });

    [Fact]
    public async Task Dado_ClienteSemPedidoNenhum_Quando_AbrirALista_Entao_DeveExplicarEOferecerOCatalogo() =>
        await Executar(async () =>
        {
            // CA-04: o oitavo cliente semeado nunca recebe pedido (spec 022
            // reservou; confirmado em T003) — mesma senha dos demais.
            await Entrar("cliente8.seed@docescabana.com.br", AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaMeusPedidos(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
            await Expect(pagina.MensagemVazia).ToContainTextAsync("Ver o catálogo");
            await Expect(pagina.Cartoes).ToHaveCountAsync(0);
        });
}
