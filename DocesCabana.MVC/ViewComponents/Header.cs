using DocesCabana.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    private readonly ICategoriaService _categoriaService;

    public HeaderViewComponent(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int itensCarrinho = 0)
    {
        ViewData["ItensCarrinho"] = itensCarrinho;

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
}
