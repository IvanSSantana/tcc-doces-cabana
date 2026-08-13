using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class LoginTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public LoginTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_ContaExistente_Quando_EntrarComEmailEComCpf_Entao_DeveAutenticarNosDois() =>
        await Executar(async () =>
        {
            var paginaLogin = new PaginaLogin(Pagina);

            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            await Sair();

            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.CpfAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");
        });

    [Fact]
    public async Task Dado_SenhaErrada_Quando_Entrar_Entao_DeveMostrarCredencialIncorreta() =>
        await Executar(async () =>
        {
            // Conta descartável, não a semeada: 5 tentativas erradas
            // bloqueiam por 15 minutos, e travar o administrador compartilhado
            // derrubaria os outros testes da suíte.
            var (email, _) = await CadastrarClienteDescartavel();

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, "SenhaErrada@123");

            await Expect(paginaLogin.MensagemDeErro).ToHaveTextAsync("E-mail ou senha incorreto(s).");
        });

    [Fact]
    public async Task Dado_Autenticado_Quando_Sair_Entao_DevePerderAcessoAAreaAdministrativa() =>
        await Executar(async () =>
        {
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            await Sair();

            await Pagina.GotoAsync($"{UrlBase}/Administrador");
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Autenticacao/Login?ReturnUrl=%2FAdministrador");
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();

    private async Task<(string Email, string Senha)> CadastrarClienteDescartavel()
    {
        var email = GeradorDeDados.EmailUnico("login");
        var dados = new DadosDeCadastro(
            "Cliente Login E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        return (email, SenhaValida);
    }
}
