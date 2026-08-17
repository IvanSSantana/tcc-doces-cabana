using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaQuemSomos
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-institucional--quem-somos");

    public PaginaQuemSomos(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) =>
        await _pagina.GotoAsync($"{urlBase}/Institucional/QuemSomos");

    public ILocator FraseDeDestaque => Container.Locator(".faixa-destaque__frase");
    public ILocator Blocos => Container.Locator(".bloco-institucional");
    public ILocator BlocoInvertido => Container.Locator(".bloco-institucional--invertido");
    public ILocator Eixo => Container.Locator(".ziguezague-institucional");
}
