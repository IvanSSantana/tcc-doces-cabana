using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaFavoritos
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-favoritos");

    public PaginaFavoritos(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Favorito");

    public ILocator Cards => Container.Locator(".card-produto");
    public ILocator MensagemVazia => Container.Locator(".favoritos-vazio");
    public ILocator LinkParaOCatalogo => MensagemVazia.GetByRole(AriaRole.Link);
}
