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

        logger.LogInformation("Starting controller action {Controller}.{Action}", controllerName, actionName);

        var resultContext = await next();

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