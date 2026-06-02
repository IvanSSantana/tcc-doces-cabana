using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class VitrineProdutosViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos)
    {
        // Conversão em lista é uma solução para o problema da múltipla enumeração
        return View(produtos.ToList());
    }
}
