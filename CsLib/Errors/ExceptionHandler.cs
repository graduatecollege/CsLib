using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Grad.CsLib.Errors;

class ExceptionHandler;

/// <summary>
/// extensions for global exception handling
/// </summary>
public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseCsLibExceptionHandler(this IApplicationBuilder app,
        ILogger? logger = null,
        bool logStructuredException = false,
        bool useGenericReason = false)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();

                if (exHandlerFeature is not null)
                {
                    logger ??= ctx.Resolve<ILogger<ExceptionHandler>>();
                    var route = exHandlerFeature.Endpoint?.DisplayName?.Split(" => ")[0];
                    var exceptionType = exHandlerFeature.Error.GetType().Name;
                    var reason = exHandlerFeature.Error.Message;
                    var ex = exHandlerFeature.Error;

                    if (logStructuredException)
                        logger.LogStructuredException(exHandlerFeature.Error, exceptionType, route, reason);
                    else
                    {
                        //this branch is only meant for unstructured textual logging
                        logger.LogUnStructuredException(exceptionType, route, reason,
                            exHandlerFeature.Error.StackTrace);
                    }

                    if (ex is BadRequestException badReqEx)
                    {
                        ctx.Response.StatusCode = 400;
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "BadRequest",
                            message = useGenericReason ? "The request was invalid." : reason
                        });
                    }
                    else if (ex is NotFoundException notFoundEx)
                    {
                        ctx.Response.StatusCode = 404;
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "NotFound",
                            message = useGenericReason ? "Not found." : reason
                        });
                    }
                    else if (ex is ForbiddenException)
                    {
                        ctx.Response.StatusCode = 403;
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "Forbidden",
                            message = useGenericReason ? "Access is forbidden." : reason
                        });
                    }
                    else
                    {
                        //for all other exceptions, we return a generic 500 error
                        ctx.Response.StatusCode = 500;
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "InternalServerError",
                            message = useGenericReason ? "An unexpected error occurred." : reason
                        });
                    }
                }
            });
        });

        return app;
    }
}