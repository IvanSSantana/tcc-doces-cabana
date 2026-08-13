using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CadastroDeClienteTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";
    private const string MensagemDeDuplicidade = "Os dados informados já estão associados a uma conta existente.";

    public CadastroDeClienteTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_DadosIneditos_Quando_CadastrarCliente_Entao_DeveLevarAoLoginEPermitirEntrar() =>
        await Executar(async () =>
        {
            var email = GeradorDeDados.EmailUnico("cliente");
            var dados = new DadosDeCadastro(
                "Cliente E2E", email, GeradorDeDados.CelularValido(), "01011995", GeradorDeDados.CpfValido(), SenhaValida);

            var paginaCadastro = new PaginaCadastro(Pagina);
            await paginaCadastro.Abrir(UrlBase);
            await paginaCadastro.Preencher(dados);
            await paginaCadastro.Enviar();

            await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Entrar(email, SenhaValida);

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");
        });

    [Fact]
    public async Task Dado_EmailJaUsado_Quando_CadastrarCliente_Entao_DeveMostrarMensagemDeDuplicidade() =>
        await Executar(async () =>
        {
            var paginaCadastro = new PaginaCadastro(Pagina);
            await paginaCadastro.Abrir(UrlBase);
            await paginaCadastro.Preencher(new DadosDeCadastro(
                "Outro Cliente", AplicacaoEmExecucao.EmailAdministrador, GeradorDeDados.CelularValido(),
                "02021996", GeradorDeDados.CpfValido(), SenhaValida));
            await paginaCadastro.Enviar();

            await Expect(paginaCadastro.MensagemDeErroGeral).ToHaveTextAsync(MensagemDeDuplicidade);
        });

    [Fact]
    public async Task Dado_CpfJaUsado_Quando_CadastrarCliente_Entao_DeveMostrarMensagemDeDuplicidade() =>
        await Executar(async () =>
        {
            var paginaCadastro = new PaginaCadastro(Pagina);
            await paginaCadastro.Abrir(UrlBase);
            await paginaCadastro.Preencher(new DadosDeCadastro(
                "Outro Cliente", GeradorDeDados.EmailUnico("outro"), GeradorDeDados.CelularValido(),
                "03031997", AplicacaoEmExecucao.CpfAdministrador, SenhaValida));
            await paginaCadastro.Enviar();

            await Expect(paginaCadastro.MensagemDeErroGeral).ToHaveTextAsync(MensagemDeDuplicidade);
        });

    [Fact]
    public async Task Dado_EmailJaUsado_Quando_CadastrarAdministrador_Entao_DeveMostrarMensagemDeDuplicidade() =>
        await Executar(async () =>
        {
            // A mesma checagem (IUsuarioService.ContaJaExiste, spec 006) vale
            // para as duas portas — é a garantia de que a 006 não regride
            // pela tela, não só pelo teste de unidade.
            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            var paginaAdmins = new PaginaAdministradores(Pagina);
            await paginaAdmins.AbrirCadastro(UrlBase);
            await paginaAdmins.PreencherCadastro(new DadosDeCadastro(
                "Outro Admin", AplicacaoEmExecucao.EmailAdministrador, GeradorDeDados.CelularValido(),
                "04041998", GeradorDeDados.CpfValido(), SenhaValida));
            await paginaAdmins.EnviarCadastro();

            await Expect(paginaAdmins.MensagemDeErroCadastro).ToHaveTextAsync(MensagemDeDuplicidade);
        });

    [Fact]
    public async Task Dado_SenhaFraca_Quando_CadastrarCliente_Entao_DeveMostrarErroDeMaiuscula() =>
        await Executar(async () =>
        {
            var paginaCadastro = new PaginaCadastro(Pagina);
            await paginaCadastro.Abrir(UrlBase);
            await paginaCadastro.Preencher(new DadosDeCadastro(
                "Cliente Senha Fraca", GeradorDeDados.EmailUnico("senhafraca"), GeradorDeDados.CelularValido(),
                "05051999", GeradorDeDados.CpfValido(), "senha123"));
            await paginaCadastro.Enviar();

            await Expect(paginaCadastro.ErroDeSenha).ToContainTextAsync("letra maiúscula");
        });
}
