using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.MVC.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocesCabana.MVC.Filters;

public class FiltroFusaoDeCarrinho : IAsyncActionFilter
{
    private readonly ICarrinhoService _carrinhoService;

    public FiltroFusaoDeCarrinho(ICarrinhoService carrinhoService)
    {
        _carrinhoService = carrinhoService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity is { IsAuthenticated: true })
        {
            var itensDaSessao = context.HttpContext.Session.Ler();
            if (itensDaSessao.Count > 0)
            {
                var usuarioId = Guid.Parse(context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _carrinhoService.Fundir(usuarioId, itensDaSessao);

                context.HttpContext.Session.Limpar();
            }
        }

        await next();
    }
}
