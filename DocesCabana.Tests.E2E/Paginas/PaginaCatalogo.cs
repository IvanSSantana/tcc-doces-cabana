using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaCatalogo
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-catalogo");

    public PaginaCatalogo(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase, string? apelido = null) =>
        await _pagina.GotoAsync(apelido is null ? $"{urlBase}/Catalogo" : $"{urlBase}/Catalogo/{apelido}");

    public ILocator Cards => Container.Locator(".card-produto");
    public ILocator MensagemVazia => Container.Locator(".catalogo-vazio");

    public ILocator BarraDePesquisa => _pagina.Locator(".barra-pesquisa input[name='termo']");

}
