using Microsoft.AspNetCore.Mvc;
using DocesCabana.Application.DTOs;

namespace DocesCabana.MVC.ViewComponents;

public class VitrineProdutosViewComponent : ViewComponent
{
    // Limite dentro do componente, não em quem o chama (RF-07 da spec 013):
    // qualquer página que use a vitrine herda o corte, sem precisar lembrar
    // de aplicá-lo. Oito produtos = cinco posições de rolagem com quatro
    // cards visíveis no desktop (spec 013 §10).
    //
    // Público desde a spec 019: quem já pede só os destaques ao armazenamento
    // (HomeController) usa a mesma constante, para o número nunca divergir
    // entre "quantos pedimos" e "quantos exibimos".
    public const int LimitePadrao = 8;

    public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos, int limite = LimitePadrao)
    {
        // Conversão em lista é uma solução para o problema da múltipla enumeração.
        // O .Take permanece mesmo com quem chama já respeitando o limite
        // (spec 019, plano §8): é a rede de segurança contra um consumidor
        // futuro que esqueça de aplicar o próprio corte.
        return View(produtos.Take(limite).ToList());
    }
}
