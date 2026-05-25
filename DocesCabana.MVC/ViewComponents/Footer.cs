using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        return Task.FromResult<IViewComponentResult>(View());
    }
}
