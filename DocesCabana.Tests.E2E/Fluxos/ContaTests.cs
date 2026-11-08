using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class ContaTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public ContaTests(FixtureE2E fixture) : base(fixture) { }

    private async Task<(string Email, string Nome, string Celular, string Cpf)> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("conta");
        var nome = "Cliente Conta E2E";
        var celular = GeradorDeDados.CelularValido();
        var cpf = GeradorDeDados.CpfValido();
        var dados = new DadosDeCadastro(nome, email, celular, "06061994", cpf, SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, SenhaValida);

        return (email, nome, celular, cpf);
    }

    private static DadosDeEndereco EnderecoValido(string rua = "Rua das Flores") =>
        new("17340-000", "São Paulo", "Barra Bonita", "Centro", rua, "123");

    [Fact]
    public async Task Dado_EnderecoDeOutraPessoa_Quando_TentarAbrirEditarOuExcluir_Entao_NaoDeveConseguir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await pagina.BotaoEditar(pagina.Cartoes.First).ClickAsync();
            var urlDeEdicaoAlheia = Pagina.Url;

            await Sair();
            await CriarClienteEEntrar();

            await Pagina.GotoAsync(urlDeEdicaoAlheia);

            await Expect(Pagina.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Não encontrado" })).ToBeVisibleAsync();
            await Expect(Pagina.Locator("input[name='Rua']")).ToHaveCountAsync(0);
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();
}
