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

            if (context.Exception is KeyNotFoundException)
            {
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is InvalidOperationException && (actionName is "VotarUtil" or "Avaliar"))
            {
                var referer = context.HttpContext.Request.Headers.Referer.ToString();
                context.Result = string.IsNullOrEmpty(referer)
                    ? new RedirectToActionResult("Index", "Home", null)
                    : new RedirectResult(referer);
                context.ExceptionHandled = true;
                return;
            }

            if (context.Exception is InvalidOperationException &&
                context.RouteData.Values["controller"]?.ToString() == "Carrinho")
            {
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new BadRequestObjectResult(new { erro = context.Exception.Message });
                }
                else
                {
                    var referer = context.HttpContext.Request.Headers.Referer.ToString();
                    context.Result = string.IsNullOrEmpty(referer)
                        ? new RedirectToActionResult("Index", "Carrinho", null)
                        : new RedirectResult(referer);
                }

                context.ExceptionHandled = true;
                return;
            }

            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
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
