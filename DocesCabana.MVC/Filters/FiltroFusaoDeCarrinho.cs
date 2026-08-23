using System.Security.Claims;
using DocesCabana.Application.Contracts.Services;
using DocesCabana.MVC.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocesCabana.MVC.Filters;

// RN-05 (spec 017): quem entra com um carrinho pendente na sessão tem os
// dois juntados — antes de a ação rodar, para que a primeira tela que a
// pessoa vir já mostre o carrinho fundido. Roda como filtro global
// (Program.cs), depois de UseAuthentication: é exatamente quando se sabe
// quem é a pessoa (plano §9, decisão contra middleware).
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

                // Limpa na mesma requisição em que funde (plano §9, risco 2)
                // — evita fundir de novo numa requisição concorrente e
                // duplicar quantidade (a soma continua limitada ao teto da
                // RN-02 de qualquer forma, mas isto evita o retrabalho).
                context.HttpContext.Session.Limpar();
            }
        }

        await next();
    }
}
