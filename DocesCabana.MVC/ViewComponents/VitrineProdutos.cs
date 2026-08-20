using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class VitrineProdutosViewComponent : ViewComponent
{
    // Limite dentro do componente, não em quem o chama (RF-07 da spec 013):
    // qualquer página que use a vitrine herda o corte, sem precisar lembrar
    // de aplicá-lo. Oito produtos = cinco posições de rolagem com quatro
    // cards visíveis no desktop (spec 013 §10).
    public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos, int limite = 8)
    {
        // Conversão em lista é uma solução para o problema da múltipla enumeração
        return View(produtos.Take(limite).ToList());
    }
}
