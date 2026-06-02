using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class CardProdutoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ProdutoDTO produto)
    {
        return View(produto);
    }
}
