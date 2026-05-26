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

            // Trata apenas requisições POST (submissão de formulário)
            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                var actionName = context.RouteData.Values["action"]?.ToString();

                // Adiciona a mensagem amigável da exceção ao ModelState
                context.ModelState.AddModelError(string.Empty, "Um erro interno ocorreu, tente novamente mais tarde.");

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
