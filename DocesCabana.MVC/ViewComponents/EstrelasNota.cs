using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

/// <summary>Nota em texto + fileira de estrelas em SVG com preenchimento fracionário (4,5 vira meia estrela de verdade).</summary>
public record EstrelasNotaModel(decimal Nota, string Tamanho);

public class EstrelasNotaViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(decimal nota, string tamanho = "pequena") =>
        View(new EstrelasNotaModel(nota, tamanho));
}
