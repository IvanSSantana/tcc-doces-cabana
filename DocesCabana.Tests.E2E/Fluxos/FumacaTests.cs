using DocesCabana.Tests.E2E.Infraestrutura;
using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Fluxos;

public class FumacaTests : TesteE2E
{
    public FumacaTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_AplicacaoNoAr_Quando_AbrirAPaginaInicial_Entao_DeveMostrarALogo() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            await Microsoft.Playwright.Assertions
                .Expect(Pagina.Locator("header").GetByAltText("Logo Doce Cabana"))
                .ToBeVisibleAsync();
        });
}
