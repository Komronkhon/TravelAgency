using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TravelAgency.Api.Filters;

public sealed class ActionExecutionTimingFilter(
    ILogger<ActionExecutionTimingFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;

        // Выполняется ПОСЛЕ Model Binding, но ДО входа в метод контроллера
        logger.LogInformation("Starting controller action {Controller}.{Action}", controllerName, actionName);

        var resultContext = await next();

        // Выполняется ПОСЛЕ выхода из метода контроллера
        stopwatch.Stop();

        context.HttpContext.Response.Headers["X-Action-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();

        if (resultContext.Exception is null)
        {
            logger.LogInformation(
                "Completed {Controller} in {ElapsedMs} ms",
                controllerName,
                stopwatch.ElapsedMilliseconds);
        }
    }
}