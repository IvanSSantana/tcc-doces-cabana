using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class CardProdutoViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ProdutoDTO produto, string rotuloBotaoCarrinho = "Adicionar")
    {
        ViewBag.RotuloBotaoCarrinho = rotuloBotaoCarrinho;
        return View(produto);
    }
}
