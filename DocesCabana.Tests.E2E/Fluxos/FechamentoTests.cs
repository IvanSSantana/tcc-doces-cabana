using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

// Achado ao escrever esta suíte, registrado em tasks.md: sem a credencial do
// MelhorEnvio (spec 020 §10, ainda não obtida), toda recotação de frete
// falha — inclusive a que os passos de Endereço/Pagamento fazem para
// mostrar as opções de entrega. Isso bloqueia, na suíte padrão, todo
// critério que depende de uma cotação bem-sucedida (CA-05, CA-07 a CA-15,
// CA-17). Os testes aqui cobrem o que é alcançável sem credencial — os
// passos, a navegação, o cadastro de endereço, e o caminho de falha de
// entrega (RF-17/CA-19), que é exatamente o que a suíte padrão sempre
// exercita de verdade. O resto fica para a Fase 8 (T048/T049 desta spec),
// marcado [Trait("Categoria", "Externo")], junto da mesma pendência que a
// 020 já tinha.
public class FechamentoTests : TesteE2E
{
    public FechamentoTests(FixtureE2E fixture) : base(fixture) { }

    // Via fetch, aguardado (mesmo caminho que SemearItem usa em outras
    // specs) — clicar no cartão e navegar em seguida corre risco de sair da
    // página antes do POST assíncrono completar.
    private async Task<Guid> AdicionarProdutoAoCarrinho()
    {
        var paginaCatalogo = new PaginaCatalogo(Pagina);
        await paginaCatalogo.Abrir(UrlBase, "doces");
        var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

        var carrinho = new PaginaCarrinho(Pagina);
        await carrinho.SemearItem(UrlBase, produtoId, 1);

        return produtoId;
    }

    // ── Os passos, e quem os vê (T047/T048) ──────────────────────────────

    [Fact]
    public async Task Dado_ClienteAutenticadoComItemNoCarrinho_Quando_AbrirATela_Entao_DeveVerOsPassosComOCarrinhoAtivo() =>
        await Executar(async () =>
        {
            // CA-01
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
            // CA-02/CA-03 (quem não entrou vê o passo; a Fase 8 confirma que
            // quem já entrou não o vê, junto do resto do caminho completo).
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
            // CA-04: a fusão de carrinhos (017) soma o que ficou na sessão
            // ao carrinho de quem entrou.
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

    // ── Endereço, sem depender de cotação real (T048) ────────────────────

    [Fact]
    public async Task Dado_ClienteSemEndereco_Quando_ChegarAoPassoDeEndereco_Entao_DeveCadastrarAliMesmoEFicarEscolhido() =>
        await Executar(async () =>
        {
            // CA-06 — o cadastro em si, e ficar escolhido, não dependem de
            // a cotação ter sucesso (essa parte fica reportada à parte,
            // pela mensagem de falha — ver o próximo teste).
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

    // ── Entrega incalculável (RF-17/CA-19) — o caminho real desta suíte ──

    [Fact]
    public async Task Dado_ServicoDeEntregaIndisponivel_Quando_ChegarAoEnderecoEscolhido_Entao_DeveAvisarSemOferecerContinuar() =>
        await Executar(async () =>
        {
            // RN-02/RF-17/CA-19: sem credencial do MelhorEnvio no ambiente
            // (spec 020 §10), a recotação sempre falha — é exatamente o
            // caminho que RF-17 pede: avisa, e não deixa prosseguir, mas o
            // carrinho segue utilizável.
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

            // O carrinho segue utilizável (RN-02 herdada da 020) — o resumo
            // (que não troca de parcial ao trocar de passo, plano §1)
            // continua visível; a lista em si só aparece no passo do
            // Carrinho, que este teste não está exibindo.
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
