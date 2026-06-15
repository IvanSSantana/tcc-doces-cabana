using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(int itensCarrinho = 0)
    {
        ViewData["ItensCarrinho"] = itensCarrinho;
        return Task.FromResult<IViewComponentResult>(View());
    }
}