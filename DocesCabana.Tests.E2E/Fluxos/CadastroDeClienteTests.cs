using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CadastroDeClienteTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

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

}
