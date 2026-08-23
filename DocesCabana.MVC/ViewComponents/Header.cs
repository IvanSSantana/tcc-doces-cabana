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
        // CA-15 (spec 017): o cabeçalho aparece em toda página, então conta
        // sozinho — autenticado, do banco; visitante, da sessão (a mesma
        // soma de quantidades que TotalDeItens usa, sem precisar buscar
        // produto nenhum: contar não valida disponibilidade).
        ViewData["ItensCarrinho"] = await ContarItensDoCarrinho();

        // A barra reexibe o termo vigente (spec 016, RF-06) lendo direto da
        // própria query string — o componente aparece em toda página, então
        // não há um "termo atual" a receber de fora: só existe quando a
        // página é o resultado de uma busca (/Catalogo?termo=...).
        ViewData["TermoDeBusca"] = Request.Query["termo"].ToString();

        // Todas as categorias no menu (RF-03 da spec 012) — a loja tem só
        // quatro hoje, então não há corte a fazer aqui, diferente do que
        // acontece dentro do catálogo com as subcategorias (RF-04).
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
