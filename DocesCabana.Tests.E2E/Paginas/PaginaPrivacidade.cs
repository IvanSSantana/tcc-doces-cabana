using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaPrivacidade
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-institucional--politica");

    public PaginaPrivacidade(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Institucional/Privacidade");

    public ILocator Titulo => Container.Locator("h1");
    public ILocator TitulosDeSecao => Container.Locator("h2");
    public ILocator LinkDoEncarregado => Container.GetByRole(AriaRole.Link, new() { Name = "privacidade@docecabana.com" });
}
