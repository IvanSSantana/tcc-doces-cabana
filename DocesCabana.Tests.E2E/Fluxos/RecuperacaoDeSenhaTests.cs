using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class RecuperacaoDeSenhaTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";
    private const string MensagemDeConfirmacao =
        "Se existir uma conta com esse login, enviamos um e-mail com o link de redefinição.";

    public RecuperacaoDeSenhaTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_LoginExistenteEInexistente_Quando_PedirRedefinicao_Entao_DeveMostrarAMesmaMensagem() =>
        await Executar(async () =>
        {
            var paginaEsqueceuSenha = new PaginaEsqueceuSenha(Pagina);

            await paginaEsqueceuSenha.Abrir(UrlBase);
            await paginaEsqueceuSenha.Solicitar(AplicacaoEmExecucao.EmailAdministrador);
            await Expect(paginaEsqueceuSenha.MensagemDeConfirmacao).ToHaveTextAsync(MensagemDeConfirmacao);

            await paginaEsqueceuSenha.Abrir(UrlBase);
            await paginaEsqueceuSenha.Solicitar(GeradorDeDados.EmailUnico("naoexiste"));
            await Expect(paginaEsqueceuSenha.MensagemDeConfirmacao).ToHaveTextAsync(MensagemDeConfirmacao);
        });

    [Fact]
    public async Task Dado_PedidoDeRedefinicao_Quando_SeguirOLinkETrocarASenha_Entao_DeveEntrarComANovaENaoComAAntiga() =>
        await Executar(async () =>
        {
            var email = GeradorDeDados.EmailUnico("redefinicao");
            var senhaNova = "OutraSenhaForte@2027";

            var paginaCadastro = new PaginaCadastro(Pagina);
            await paginaCadastro.Abrir(UrlBase);
            await paginaCadastro.Preencher(new DadosDeCadastro(
                "Cliente Redefinicao", email, GeradorDeDados.CelularValido(), "07071993", GeradorDeDados.CpfValido(), SenhaValida));
            await paginaCadastro.Enviar();
            await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

            var paginaEsqueceuSenha = new PaginaEsqueceuSenha(Pagina);
            await paginaEsqueceuSenha.Abrir(UrlBase);
            await paginaEsqueceuSenha.Solicitar(email);
            await Expect(paginaEsqueceuSenha.MensagemDeConfirmacao).ToHaveTextAsync(MensagemDeConfirmacao);

            var link = await CaixaDeEntrada.EsperarLinkDeRedefinicao(Aplicacao.PastaDeEmails, email);

            var paginaRedefinir = new PaginaRedefinirSenha(Pagina);
            await paginaRedefinir.AbrirPeloLink(link);
            await paginaRedefinir.DefinirNovaSenha(senhaNova);
            await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

            var paginaLogin = new PaginaLogin(Pagina);

            await paginaLogin.Entrar(email, senhaNova);
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/");

            // Confirma que a senha antiga deixou de valer — RF-07 pede a
            // troca de ponta a ponta, não só que a nova funcione.
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);
            await Expect(paginaLogin.MensagemDeErro).ToHaveTextAsync("E-mail ou senha incorreto(s).");
        });
}
