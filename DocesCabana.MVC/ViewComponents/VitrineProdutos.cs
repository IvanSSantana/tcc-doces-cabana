using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class VitrineProdutosViewComponent : ViewComponent
{
    public const int LimitePadrao = 8;

    public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos, int limite = LimitePadrao)
    {
        return View(produtos.Take(limite).ToList());
    }
}
