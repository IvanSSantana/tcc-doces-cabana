using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Controllers;

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
