using DocesCabana.Tests.E2E.Infraestrutura;
using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Fluxos;

/// <summary>
/// Prova o andaime, não um requisito da spec: se este teste não passar, nada
/// nas fases seguintes vai passar, e por motivos que não têm a ver com os
/// fluxos (T017 da 007).
/// </summary>
public class FumacaTests : TesteE2E
{
    public FumacaTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_AplicacaoNoAr_Quando_AbrirAPaginaInicial_Entao_DeveMostrarALogo() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            // A página tem duas logos — a do cabeçalho e a do modal de login,
            // escondido mas presente no DOM — por isso escopar em <header>
            // (mesma colisão que o plano §8 previu para os links "Entrar").
            await Microsoft.Playwright.Assertions
                .Expect(Pagina.Locator("header").GetByAltText("Logo Doce Cabana"))
                .ToBeVisibleAsync();
        });
}
