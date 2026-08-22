using DocesCabana.Tests.E2E.Infraestrutura;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

/// <summary>
/// Não-regressão do desenho compartilhado de formulário (spec 016, CA-19).
///
/// Este teste é escrito e verificado verde ANTES de qualquer CSS ser tocado
/// (plano §7): ele mede o que as cinco telas já são, para que a extração de
/// <c>autenticacao.css</c> em <c>components/formulario.css</c> (T048-T050)
/// não possa mudar a aparência delas sem que este teste denuncie. As telas
/// medidas não incluem o cadastro de produto — esse é o que a feature
/// redesenha de propósito (RF-16), coberto por CadastroDeProdutoTests.
/// </summary>
public class FormularioTests : TesteE2E
{
    public FormularioTests(FixtureE2E fixture) : base(fixture) { }

    public static IEnumerable<object[]> TelasDeFormulario()
    {
        yield return ["/Autenticacao/Login"];
        yield return ["/Autenticacao/Cadastro"];
        yield return ["/Autenticacao/EsqueceuSenha"];
        yield return ["/Autenticacao/RedefinirSenha?token=teste&email=teste@docescabana.com.br"];
        yield return ["/Admin/Administrador/Cadastro"];
    }

    [Theory]
    [MemberData(nameof(TelasDeFormulario))]
    public async Task Dado_UmaTelaDeFormulario_Quando_MedirOCampoDeTexto_Entao_DeveSeguirODesenhoDaMarca(string caminho) =>
        await Executar(async () =>
        {
            // RedefinirSenha e Cadastro de Administrador ficam atrás de
            // autenticação/autorização, mas ambas renderizam a marcação
            // independentemente de token/sessão válidos — só o POST exige
            // isso. Medir o GET é suficiente para o desenho.
            if (caminho.StartsWith("/Admin"))
            {
                var login = new Paginas.PaginaLogin(Pagina);
                await login.Abrir(UrlBase);
                await login.Entrar(AplicacaoEmExecucao.EmailAdministrador, AplicacaoEmExecucao.SenhaAdministrador);
            }

            await Pagina.GotoAsync($"{UrlBase}{caminho}");

            var container = Pagina.Locator(".container-autenticacao");
            await Expect(container).ToBeVisibleAsync();

            var titulo = container.Locator(".titulo-tela").First;
            await Expect(titulo).ToBeVisibleAsync();

            var campos = container.Locator(".campo-texto");
            var primeiroCampo = campos.First;
            await Expect(primeiroCampo).ToBeVisibleAsync();

            // Largura e altura do campo de texto, raio da borda e
            // espaçamento entre campos — exatamente o que T004 pede medir.
            var estiloDoCampo = await primeiroCampo.EvaluateAsync<string[]>(@"el => {
                const estilo = getComputedStyle(el);
                return [estilo.height, estilo.borderRadius, estilo.backgroundColor];
            }");

            Assert.Equal("54px", estiloDoCampo[0]);
            Assert.Equal("20px", estiloDoCampo[1]);
            Assert.Equal("rgb(255, 255, 255)", estiloDoCampo[2]);

            var larguraDoCampo = await primeiroCampo.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            var larguraDoContainer = await container.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
            // O campo preenche a largura do container de formulário — a
            // proporção é o que se mede, não um valor fixo, porque a largura
            // do container varia com o viewport do teste.
            Assert.True(larguraDoCampo >= larguraDoContainer * 0.9,
                $"Campo esperado preenchendo o container ({larguraDoContainer}px), mediu {larguraDoCampo}px.");

            var espacamentoEntreCampos = await container.Locator(".formulario-autenticacao").First.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            Assert.Equal("24px", espacamentoEntreCampos);

            var estiloDoRotulo = await container.Locator(".campo-entrada label").First.EvaluateAsync<string[]>(@"el => {
                const estilo = getComputedStyle(el);
                return [estilo.color, estilo.fontWeight];
            }");

            Assert.Equal("rgb(5, 92, 64)", estiloDoRotulo[0]);
            Assert.Equal("600", estiloDoRotulo[1]);

            var estiloDoTitulo = await titulo.EvaluateAsync<string[]>(@"el => {
                const estilo = getComputedStyle(el);
                return [estilo.color, estilo.fontWeight];
            }");

            Assert.Equal("rgb(5, 92, 64)", estiloDoTitulo[0]);
            Assert.Equal("600", estiloDoTitulo[1]);
        });
}
