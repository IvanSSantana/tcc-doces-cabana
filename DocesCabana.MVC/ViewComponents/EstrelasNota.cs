using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

public record EstrelasNotaModel(decimal Nota, string Tamanho);

public class EstrelasNotaViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(decimal nota, string tamanho = "pequena") =>
        View(new EstrelasNotaModel(nota, tamanho));
}
