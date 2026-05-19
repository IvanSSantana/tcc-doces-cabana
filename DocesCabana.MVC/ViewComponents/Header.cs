namespace DocesCabana.MVC.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(int itensCarrinho = 0, int tipoHeader = 2 )
    {
        ViewData["ItensCarrinho"] = itensCarrinho;
        ViewData["TipoHeader"] = tipoHeader;
        return Task.FromResult<IViewComponentResult>(View());
    }
}