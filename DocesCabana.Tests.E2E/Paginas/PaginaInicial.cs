using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaInicial
{
    private readonly IPage _pagina;

    public PaginaInicial(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync(urlBase);

    public ILocator BarraDePesquisa => _pagina.Locator(".barra-pesquisa input[name='termo']");
    public ILocator BotaoPesquisar => _pagina.Locator(".botao-pesquisar");

    public async Task Buscar(string termo)
    {
        await BarraDePesquisa.FillAsync(termo);
        await BotaoPesquisar.ClickAsync();
    }
}
