using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class PaginasInstitucionaisTests : TesteE2E
{
    public PaginasInstitucionaisTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_VisitanteNaPaginaInicial_Quando_ClicarNaPoliticaDoRodape_Entao_DeveAbrirAPolitica() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            await Pagina.Locator("footer").GetByRole(AriaRole.Link, new() { Name = "Política de Privacidade" }).ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Institucional/Privacidade");
            await Expect(new PaginaPrivacidade(Pagina).Titulo).ToHaveTextAsync("Política de Privacidade");
        });

    [Fact]
    public async Task Dado_ModalDeLoginAberto_Quando_ClicarNaPolitica_Entao_DeveAbrirAMesmaPagina() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);
            // O <a onclick="abrirModal()"> do cabeçalho não tem href — sem
            // href, um <a> não expõe papel ARIA "link" (regra do HTML), por
            // isso o locator é por texto, não por role, aqui.
            await Pagina.Locator("header").GetByText("Entrar", new() { Exact = true }).ClickAsync();

            await Pagina.Locator("#modal-login").GetByRole(AriaRole.Link, new() { Name = "Política de Privacidade" }).ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Institucional/Privacidade");
        });

    [Fact]
    public async Task Dado_VisitanteNaPaginaInicial_Quando_ClicarEmQuemSomosNoRodape_Entao_DeveAbrirQuemSomos() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            await Pagina.Locator("footer").GetByRole(AriaRole.Link, new() { Name = "Quem Somos" }).ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Institucional/QuemSomos");
            await Expect(new PaginaQuemSomos(Pagina).FraseDeDestaque).ToContainTextAsync("Revivendo os sabores da nossa");
        });

    [Fact]
    public async Task Dado_PaginaDePolitica_Quando_ListarOsTitulosDeSecao_Entao_DeveTrazerAsOnzeNaOrdem() =>
        await Executar(async () =>
        {
            var pagina = new PaginaPrivacidade(Pagina);
            await pagina.Abrir(UrlBase);

            var titulos = await pagina.TitulosDeSecao.AllTextContentsAsync();

            Assert.Equal(new[]
            {
                "Definições",
                "Quais dados pessoais coletamos?",
                "Qual o objetivo do tratamento de dados?",
                "Quando e como coletamos seus dados?",
                "Compartilhamento de dados pessoais",
                "Por quanto tempo armazenamos os dados?",
                "Tratamento de dados de menores de idade",
                "Direitos dos titulares de dados",
                "Como solicitar seus direitos",
                "Atualizações desta Política",
                "Contato",
            }, titulos);
        });

    [Fact]
    public async Task Dado_SecaoDeContato_Quando_InspecionarOEmail_Entao_DeveSerUmLinkMailto() =>
        await Executar(async () =>
        {
            var pagina = new PaginaPrivacidade(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.LinkDoEncarregado).ToHaveAttributeAsync("href", "mailto:privacidade@docecabana.com");
        });

    [Fact]
    public async Task Dado_PaginaQuemSomos_Quando_ListarOsBlocos_Entao_DeveTrazerMissaoPropositoEVisao() =>
        await Executar(async () =>
        {
            var pagina = new PaginaQuemSomos(Pagina);
            await pagina.Abrir(UrlBase);

            var titulos = await pagina.Blocos.Locator(".bloco-institucional__titulo").AllTextContentsAsync();

            Assert.Equal(new[] { "Missão", "Propósito", "Visão" }, titulos);
        });

    [Fact]
    public async Task Dado_PaginaQuemSomosEmTelaLarga_Quando_CompararOsBlocos_Entao_OPropositoDeveEstarInvertido() =>
        await Executar(async () =>
        {
            await Pagina.SetViewportSizeAsync(1280, 900);
            var pagina = new PaginaQuemSomos(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.BlocoInvertido).ToHaveCountAsync(1);
            var tituloInvertido = await pagina.BlocoInvertido.Locator(".bloco-institucional__titulo").TextContentAsync();
            Assert.Equal("Propósito", tituloInvertido);

            // O eixo é estrutura do ziguezague (RN-05): existe enquanto há duas colunas.
            var existeEixo = await pagina.Eixo.EvaluateAsync<bool>(
                "el => getComputedStyle(el, '::before').display !== 'none'");
            Assert.True(existeEixo);
        });

    [Fact]
    public async Task Dado_RotaAntigaDePrivacidade_Quando_Acessada_Entao_DeveResponder404() =>
        await Executar(async () =>
        {
            var resposta = await Pagina.GotoAsync($"{UrlBase}/Home/Privacidade");

            Assert.Equal(404, resposta!.Status);
        });

    [Fact]
    public async Task Dado_TelaDe375px_Quando_AbrirCadaPagina_Entao_NaoDeveHaverRolagemHorizontal() =>
        await Executar(async () =>
        {
            // Escopado ao conteúdo desta feature (.pagina-institucional), não
            // ao documento inteiro: o cabeçalho compartilhado já estoura a
            // 375px antes desta feature existir — achado registrado no
            // checklist, fora do escopo declarado (spec §8), não silenciado
            // nem corrigido aqui.
            await Pagina.SetViewportSizeAsync(375, 800);

            await new PaginaPrivacidade(Pagina).Abrir(UrlBase);
            await SemRolagemHorizontal();

            await new PaginaQuemSomos(Pagina).Abrir(UrlBase);
            await SemRolagemHorizontal();
        });

    [Fact]
    public async Task Dado_CadaPaginaInstitucional_Quando_ProcurarFormulario_Entao_NaoDeveHaverNenhum() =>
        await Executar(async () =>
        {
            await new PaginaPrivacidade(Pagina).Abrir(UrlBase);
            await Expect(Pagina.Locator("main form")).ToHaveCountAsync(0);

            await new PaginaQuemSomos(Pagina).Abrir(UrlBase);
            await Expect(Pagina.Locator("main form")).ToHaveCountAsync(0);
        });

    private async Task SemRolagemHorizontal()
    {
        var larguras = await Pagina.EvaluateAsync<int[]>(@"() => {
            const el = document.querySelector('.pagina-institucional');
            return [el.scrollWidth, document.documentElement.clientWidth];
        }");
        Assert.True(larguras[0] <= larguras[1], $"scrollWidth={larguras[0]} > clientWidth={larguras[1]}");
    }
}
