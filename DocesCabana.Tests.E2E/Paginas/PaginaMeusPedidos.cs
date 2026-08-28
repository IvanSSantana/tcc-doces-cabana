using Microsoft.Playwright;

namespace DocesCabana.Tests.E2E.Paginas;

public class PaginaMeusPedidos
{
    private readonly IPage _pagina;
    private ILocator Container => _pagina.Locator(".pagina-conta");

    public PaginaMeusPedidos(IPage pagina) => _pagina = pagina;

    public async Task Abrir(string urlBase) => await _pagina.GotoAsync($"{urlBase}/Pedido/Meus");

    public ILocator LinkMeusPedidosNoMenu => Container.Locator(".menu-conta").GetByRole(AriaRole.Link, new() { Name = "Meus pedidos" });
    public ILocator MensagemVazia => Container.Locator(".pedidos-vazio");
    public ILocator Cartoes => Container.Locator(".cartao-pedido");

    public async Task AbrirPrimeiroPedido() => await Cartoes.First.ClickAsync();
}
