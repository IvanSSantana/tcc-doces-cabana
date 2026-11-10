using DocesCabana.MVC.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocesCabana.Tests.Units.Filters;

public class FilterExceptionTests
{
    private readonly FilterException _filtro = new(
        new Mock<ILogger<FilterException>>().Object,
        new Mock<IModelMetadataProvider>().Object);

    private static ActionExecutedContext CriarContexto(string acao, Exception excecao, string? referer = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        if (referer is not null)
            httpContext.Request.Headers.Referer = referer;

        var rotas = new RouteData();
        rotas.Values["controller"] = "Produto";
        rotas.Values["action"] = acao;

        var contextoDaAcao = new ActionContext(httpContext, rotas, new ActionDescriptor());
        return new ActionExecutedContext(contextoDaAcao, [], new object()) { Exception = excecao };
    }

    [Theory]
    [InlineData("VotarUtil")]
    [InlineData("Avaliar")]
    public void Dado_InvalidOperationEmAcaoDeAvaliacaoComReferer_Quando_OnActionExecuted_Entao_DeveRedirecionarParaOReferer(string acao)
    {
        var contexto = CriarContexto(acao, new InvalidOperationException("Você já comentou sobre esse produto."),
            referer: "https://loja.test/Produto/Detalhes/abc");

        _filtro.OnActionExecuted(contexto);

        var redirect = Assert.IsType<RedirectResult>(contexto.Result);
        Assert.Equal("https://loja.test/Produto/Detalhes/abc", redirect.Url);
        Assert.True(contexto.ExceptionHandled);
    }

    [Fact]
    public void Dado_InvalidOperationEmAvaliarSemReferer_Quando_OnActionExecuted_Entao_DeveRedirecionarParaAHome()
    {
        var contexto = CriarContexto("Avaliar", new InvalidOperationException("Só quem recebeu este produto pode avaliá-lo."));

        _filtro.OnActionExecuted(contexto);

        var redirect = Assert.IsType<RedirectToActionResult>(contexto.Result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }
}
