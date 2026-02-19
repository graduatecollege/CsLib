using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Grad.CsLib.ClientErrorLogging;

class ClientErrorLogger;

/// <summary>
/// Provides extension methods for <see cref="WebApplication"/> to add client-side error logging endpoints.
/// </summary>
public static class ClientErrorLoggingExtensions
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Maps a client-side error logging endpoint to the specified path.
        /// The endpoint accepts POST requests with error type, message, stacktrace, and contextual data,
        /// and logs them to Serilog for collection by Splunk.
        /// </summary>
        /// <param name="path">The path where the endpoint should be mapped (e.g., "/api/client-errors").</param>
        public WebApplication MapClientErrorLogging(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            app.MapPost(path,
                    async (ClientErrorLog req, ILogger<ClientErrorLogger> endpointLogger) =>
                    {
                        var validationResult = await new ClientErrorLogValidator().ValidateAsync(req);

                        if (!validationResult.IsValid)
                        {
                            return Results.BadRequest(new
                            {
                                error = "ValidationFailed",
                                errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                            });
                        }

                        endpointLogger.LogError(
                            "Client error: Type={ErrorType}, Message={Message}, StackTrace={StackTrace}, Context={@Context}",
                            req.ErrorType,
                            req.Message,
                            req.StackTrace ?? "N/A",
                            req.Context ?? new Dictionary<string, object?>()
                        );
                        
                        Log.Error("Client error logged: {@ClientErrorLog}", req);

                        return Results.Ok(new { });
                    })
                .AllowAnonymous()
                .WithName("ClientErrorLog")
                .WithSummary("Log client-side errors")
                .WithDescription("Accepts client-side error logs for server-side logging and analysis");

            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ClientErrorLogging");
            logger.LogInformation("Client error logging endpoint mapped to {Path}", path);
            return app;
        }
    }
}
