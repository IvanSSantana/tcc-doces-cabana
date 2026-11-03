using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class FechamentoTests : TesteE2E
{
    public FechamentoTests(FixtureE2E fixture) : base(fixture) { }

    private async Task<Guid> AdicionarProdutoAoCarrinho()
    {
        var paginaCatalogo = new PaginaCatalogo(Pagina);
        await paginaCatalogo.Abrir(UrlBase, "doces");
        var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

        var carrinho = new PaginaCarrinho(Pagina);
        await carrinho.SemearItem(UrlBase, produtoId, 1);

        return produtoId;
    }

    [Fact]
    public async Task Dado_ClienteAutenticadoComItemNoCarrinho_Quando_AbrirATela_Entao_DeveVerOsPassosComOCarrinhoAtivo() =>
        await Executar(async () =>
        {
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);
            await AdicionarProdutoAoCarrinho();

            var carrinho = new PaginaCarrinho(Pagina);
            var fechamento = new PaginaFechamento(Pagina);
            await carrinho.Abrir(UrlBase);

            await Expect(fechamento.PassoAtivo).ToHaveTextAsync("Carrinho");
            await Expect(carrinho.BotaoFinalizar).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Visitante_Quando_AvancarDoCarrinho_Entao_DeveEncontrarOPassoDeEntrar() =>
        await Executar(async () =>
        {
            await AdicionarProdutoAoCarrinho();

            var carrinho = new PaginaCarrinho(Pagina);
            await carrinho.Abrir(UrlBase);

            var fechamento = new PaginaFechamento(Pagina);
            await carrinho.BotaoFinalizar.ClickAsync();

            await Expect(fechamento.PassoAtivo).ToHaveTextAsync("Conta");
            await Expect(fechamento.LinkEntrar).ToBeVisibleAsync();
            await Expect(fechamento.LinkCriarConta).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_EntreiNoMeioDoFechamento_Quando_ATelaVoltar_Entao_DevoEstarNoPassoDoCarrinho() =>
        await Executar(async () =>
        {
            var produtoId = await AdicionarProdutoAoCarrinho();
            var carrinho = new PaginaCarrinho(Pagina);
            await carrinho.Abrir(UrlBase);

            var fechamento = new PaginaFechamento(Pagina);
            await carrinho.BotaoFinalizar.ClickAsync();
            await fechamento.LinkEntrar.ClickAsync();

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Carrinho");
            await Expect(carrinho.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ClienteSemEndereco_Quando_ChegarAoPassoDeEndereco_Entao_DeveCadastrarAliMesmoEFicarEscolhido() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            await AdicionarProdutoAoCarrinho();

            var carrinho = new PaginaCarrinho(Pagina);
            await carrinho.Abrir(UrlBase);
            var fechamento = new PaginaFechamento(Pagina);
            await carrinho.BotaoFinalizar.ClickAsync();

            await Expect(fechamento.PassoAtivo).ToHaveTextAsync("Endereço");
            await Expect(fechamento.ListaDeEnderecos).ToHaveCountAsync(0);

            await fechamento.CadastrarEnderecoAqui("17340001", "Rua Nova", "42");

            await Expect(fechamento.EnderecoSelecionado).ToBeVisibleAsync();
            await Expect(fechamento.EnderecoSelecionado).ToContainTextAsync("Rua Nova");
        });

    [Fact]
    public async Task Dado_ServicoDeEntregaIndisponivel_Quando_ChegarAoEnderecoEscolhido_Entao_DeveAvisarSemOferecerContinuar() =>
        await Executar(async () =>
        {
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);
            var produtoId = await AdicionarProdutoAoCarrinho();

            var carrinho = new PaginaCarrinho(Pagina);
            var fechamento = new PaginaFechamento(Pagina);
            await carrinho.Abrir(UrlBase);
            await carrinho.BotaoFinalizar.ClickAsync();

            await Expect(fechamento.PassoAtivo).ToHaveTextAsync("Endereço");
            await Expect(Pagina.Locator(".mensagem-falha-frete")).ToContainTextAsync("Não foi possível calcular o frete agora");
            await Expect(fechamento.LinkContinuarParaPagamento).ToHaveCountAsync(0);

            await Expect(Pagina.Locator(".total-itens-carrinho")).ToBeVisibleAsync();

            await carrinho.Abrir(UrlBase);
            await Expect(carrinho.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    private async Task<string> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("fechamento");
        var dados = new DadosDeCadastro(
            "Cliente Fechamento E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), "SenhaForte@2026");

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
