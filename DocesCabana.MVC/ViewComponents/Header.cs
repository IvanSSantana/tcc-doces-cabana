using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    private readonly ICategoriaService _categoriaService;
    private readonly ICarrinhoService _carrinhoService;

    public HeaderViewComponent(ICategoriaService categoriaService, ICarrinhoService carrinhoService)
    {
        _categoriaService = categoriaService;
        _carrinhoService = carrinhoService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ViewData["ItensCarrinho"] = await ContarItensDoCarrinho();

        ViewData["TermoDeBusca"] = Request.Query["termo"].ToString();

        var categorias = await _categoriaService.ListarComSubcategorias();

        return View(categorias);
    }

    private async Task<int> ContarItensDoCarrinho()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            var id = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            return id is null ? 0 : await _carrinhoService.ContarItens(Guid.Parse(id));
        }

        return HttpContext.Session.Ler().Sum(i => (int)i.Quantidade);
    }
}
