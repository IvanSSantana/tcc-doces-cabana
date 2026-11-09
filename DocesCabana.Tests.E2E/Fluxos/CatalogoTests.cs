using System.Text.RegularExpressions;
using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CatalogoTests : TesteE2E
{
    public CatalogoTests(FixtureE2E fixture) : base(fixture) { }

    [Fact]
    public async Task Dado_Visitante_Quando_BuscarPeloNomeEAbrirOProduto_Entao_DeveChegarNaPaginaDele() =>
        await Executar(async () =>
        {
            var inicial = new PaginaInicial(Pagina);
            await inicial.Abrir(UrlBase);

            await inicial.Buscar("Raspa Tacho");

            var catalogo = new PaginaCatalogo(Pagina);
            await Expect(catalogo.Cards).ToHaveCountAsync(1);

            await catalogo.Cards.First.Locator(".nome-card").ClickAsync();

            await Expect(Pagina).ToHaveURLAsync(new Regex($"{Regex.Escape(UrlBase)}/Produto/Detalhes/.+"));
        });

}
