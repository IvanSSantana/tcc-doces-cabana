using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

// Páginas institucionais (spec 009): conteúdo fixo, público, sem estado.
// Nenhuma dependência injetada — não há camada de aplicação a consultar.
public class InstitucionalController : Controller
{
    [HttpGet]
    public IActionResult Privacidade()
    {
        return View();
    }

    [HttpGet]
    public IActionResult QuemSomos()
    {
        return View();
    }
}
