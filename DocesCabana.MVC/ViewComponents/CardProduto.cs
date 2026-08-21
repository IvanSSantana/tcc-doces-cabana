using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class CardProdutoViewComponent : ViewComponent
{
    // Rótulo do botão de carrinho (spec 015, RF-15): o catálogo passa
    // "Adicionar ao carrinho", fiel à referência visual; o padrão mantém
    // "Adicionar", que é o que o carrossel sempre teve, sem que ele precise
    // saber que o parâmetro existe (RF-17).
    public IViewComponentResult Invoke(ProdutoDTO produto, string rotuloBotaoCarrinho = "Adicionar")
    {
        ViewBag.RotuloBotaoCarrinho = rotuloBotaoCarrinho;
        return View(produto);
    }
}
