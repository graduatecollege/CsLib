using FastEndpoints;

namespace Grad.CsLib.Errors;

public class ExceptionProcessor : IGlobalPostProcessor 
{
    public async Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
    {
        if (!ctx.HasExceptionOccurred)
            return;

        if (ctx.ExceptionDispatchInfo.SourceException.GetType() == typeof(BadRequestException))
        {
            ctx.MarkExceptionAsHandled();
            var ex = ctx.ExceptionDispatchInfo.SourceException;
            await ctx.HttpContext.Response.SendAsync(ex.Message, 400, cancellation: ct);
            return;
        }
        if (ctx.ExceptionDispatchInfo.SourceException.GetType() == typeof(NotFoundException))
        {
            ctx.MarkExceptionAsHandled();
            var ex = ctx.ExceptionDispatchInfo.SourceException;
            await ctx.HttpContext.Response.SendAsync(ex.Message, 404, cancellation: ct);
            return;
        }

        ctx.ExceptionDispatchInfo.Throw();
    }
}