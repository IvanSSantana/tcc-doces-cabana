using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DocesCabana.MVC.Filters;

public class FilterException : IActionFilter
{
    private readonly ILogger<FilterException> _logger;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public FilterException(ILogger<FilterException> logger, IModelMetadataProvider modelMetadataProvider)
    {
        _logger = logger;
        _modelMetadataProvider = modelMetadataProvider;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Salva o primeiro argumento (geralmente o DTO/Model do formulário) para uso caso ocorra erro
        var model = context.ActionArguments.Values.FirstOrDefault();

        if (model is not null)
        {
            context.HttpContext.Items["MvcModel"] = model;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null && !context.ExceptionHandled)
        {
            _logger.LogError(context.Exception, "Erro tratado pelo FilterException na ação: {Action}", context.RouteData.Values["action"]);
            _logger.LogError(context.Exception.Message);

            var actionName = context.RouteData.Values["action"]?.ToString();

            // Recurso ausente ou inativo (spec 008, RF-03/RF-04): 404 em
            // qualquer método, não só POST. app.UseStatusCodePagesWithReExecute
            // reexecuta em /Home/NaoEncontrado por cima do resultado vazio.
            if (context.Exception is KeyNotFoundException)
            {
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                return;
            }

            // ProdutoController.VotarUtil não tem view própria — sempre
            // redireciona no sucesso. Se o voto for recusado (RF-21: autor
            // votando na própria avaliação), não há para onde a lógica
            // genérica abaixo redesenhar; volta para de onde veio.
            if (context.Exception is InvalidOperationException && actionName == "VotarUtil")
            {
                var referer = context.HttpContext.Request.Headers.Referer.ToString();
                context.Result = string.IsNullOrEmpty(referer)
                    ? new RedirectToActionResult("Index", "Home", null)
                    : new RedirectResult(referer);
                context.ExceptionHandled = true;
                return;
            }

            // Trata apenas requisições POST (submissão de formulário)
            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                // Adiciona a mensagem amigável da exceção ao ModelState
                if (context.Exception is InvalidOperationException ex)
                {
                    context.ModelState.AddModelError(
                        string.Empty,
                        ex.Message);
                }
                else
                {
                    context.ModelState.AddModelError(
                        string.Empty,
                        "Um erro interno ocorreu, tente novamente mais tarde.");
                }

                var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);

                // Recupera o model salvo anteriormente para preencher a tela novamente
                if (context.HttpContext.Items.TryGetValue("MvcModel", out var model))
                {
                    viewData.Model = model;
                }

                context.Result = new ViewResult
                {
                    ViewName = actionName,
                    ViewData = viewData
                };

                context.ExceptionHandled = true;
            }
        }
    }
}
