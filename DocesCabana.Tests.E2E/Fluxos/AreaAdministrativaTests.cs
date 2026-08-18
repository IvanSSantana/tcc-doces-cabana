using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class AreaAdministrativaTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public AreaAdministrativaTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_Visitante_Quando_AbrirAreaAdministrativa_Entao_DeveLevarAoLogin() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync($"{UrlBase}/Catalogo/Cadastro");
            Assert.Contains("/Autenticacao/Login", Pagina.Url);

            await Pagina.GotoAsync($"{UrlBase}/Administrador");
            Assert.Contains("/Autenticacao/Login", Pagina.Url);
        });

    [Fact]
    public async Task Dado_ClienteComum_Quando_AbrirAreaAdministrativa_Entao_DeveReceberAcessoNegado() =>
        await Executar(async () =>
        {
            await CadastrarEEntrarComoClienteComum();

            await Pagina.GotoAsync($"{UrlBase}/Catalogo/Cadastro");
            Assert.Contains("/Home/AcessoNegado", Pagina.Url);

            await Pagina.GotoAsync($"{UrlBase}/Administrador");
            Assert.Contains("/Home/AcessoNegado", Pagina.Url);
        });

    [Fact]
    public async Task Dado_ClienteComum_Quando_OlharOCabecalho_Entao_NaoDeveVerCaminhoAdministrativo() =>
        await Executar(async () =>
        {
            await CadastrarEEntrarComoClienteComum();

            await Expect(Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Administradores" }))
                .Not.ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Administrador_Quando_CadastrarOutroAdministrador_Entao_ELeDeveEntrarEUsarAArea() =>
        await Executar(async () =>
        {
            var email = GeradorDeDados.EmailUnico("novoadmin");
            var dados = new DadosDeCadastro(
                "Novo Admin E2E", email, GeradorDeDados.CelularValido(), "08081992", GeradorDeDados.CpfValido(), SenhaValida);

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            var paginaAdmins = new PaginaAdministradores(Pagina);
            await paginaAdmins.AbrirIndice(UrlBase);
            // O próprio administrador semeado consta na lista (CA-01 da 005,
            // reconfirmado aqui como pré-condição).
            await Expect(paginaAdmins.LinhaComEmail(AplicacaoEmExecucao.EmailAdministrador)).ToBeVisibleAsync();

            await paginaAdmins.IrParaCadastro();
            await paginaAdmins.PreencherCadastro(dados);
            await paginaAdmins.EnviarCadastro();

            await Expect(paginaAdmins.MensagemDeConfirmacao).ToHaveTextAsync("Administrador cadastrado com sucesso!");
            await Expect(paginaAdmins.LinhaComEmail(email)).ToBeVisibleAsync();

            await Sair();

            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            // "usa a área administrativa" — não só entra, também acessa sem
            // ser barrado (CA-03 da 005, provado de novo pela porta nova).
            await paginaAdmins.AbrirIndice(UrlBase);
            Assert.Contains("/Administrador", Pagina.Url);
            Assert.DoesNotContain("AcessoNegado", Pagina.Url);
        });

    [Fact]
    public async Task Dado_EnderecoAntigoDeCadastroDeProduto_Quando_Acessado_Entao_DeveResponder404() =>
        await Executar(async () =>
        {
            // O endereço antigo (`AdminController`, renomeado para
            // `CatalogoController` na 010) não existe mais — nem para quem
            // está autenticado como administrador, que é quem tinha motivo
            // para acessá-lo.
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            var resposta = await Pagina.GotoAsync($"{UrlBase}/Admin/Cadastro");

            Assert.Equal(404, resposta!.Status);
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();

    private async Task CadastrarEEntrarComoClienteComum()
    {
        var email = GeradorDeDados.EmailUnico("comum");
        var dados = new DadosDeCadastro(
            "Cliente Comum E2E", email, GeradorDeDados.CelularValido(), "09091991", GeradorDeDados.CpfValido(), SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, SenhaValida);
        await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");
    }
}
