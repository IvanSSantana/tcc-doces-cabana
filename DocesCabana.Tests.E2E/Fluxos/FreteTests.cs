using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

// Os três critérios que não dependem de credencial (spec 020, tasks.md
// Fase 6): a barreira de entrada e o tratamento de falha resolvem antes de
// qualquer rede real. O restante — cotação de verdade contra a API — fica
// para a Fase 8 (T048/T049), marcada [Trait("Categoria", "Externo")].
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
            // CA-10
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.CalcularFrete("123");

            await Expect(pagina.ErroCep).ToBeVisibleAsync();
            // O carrinho segue inteiro — o item não some, nem o subtotal.
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CarrinhoVazio_Quando_AbrirATela_Entao_NaoDeveOferecerOCalculoDeFrete() =>
        await Executar(async () =>
        {
            // CA-13
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
            // CA-11 — a aplicação sobe sem credencial (AplicacaoEmExecucao
            // aponta FreteSettings:UrlBase para um endereço que recusa
            // conexão quando não há FreteSettings__Token no ambiente de
            // quem executa), então qualquer CEP válido já exercita esse
            // caminho de falha de verdade, sem precisar simular nada aqui.
            //
            // Sem JavaScript de propósito: isola o comportamento do
            // servidor (RN-02) do comportamento do script — o mesmo
            // caminho que a Fase 8 (CA-14) vai exigir functionando.
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
            // O carrinho segue utilizável — o item continua lá, e dá para
            // seguir alterando quantidade normalmente.
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
