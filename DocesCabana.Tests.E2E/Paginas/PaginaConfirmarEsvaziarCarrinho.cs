using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

// Caminho sem JavaScript da RF-11 (spec 021) — página própria, não diálogo:
// /Carrinho/ConfirmarEsvaziar.
public class PaginaConfirmarEsvaziarCarrinho
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-confirmar-esvaziar");

    public PaginaConfirmarEsvaziarCarrinho(IPage pagina) => _pagina = pagina;

    public ILocator BotaoConfirmar => Container.Locator(".botao-confirmar-esvaziar");
    public ILocator LinkCancelar => Container.Locator(".botao-cancelar-esvaziar");
}
