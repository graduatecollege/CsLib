using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Grad.CsLib.Errors;
using Grad.CsLib.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using NSwag;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;

namespace Grad.CsLib;

/// <summary>
/// Provides extension methods for <see cref="WebApplicationBuilder"/> to simplify common web application setup
/// based on Graduate College standards.
/// </summary>
public static class CsLibWeb
{
    private const string SwaggerKey = "CsLibWeb:Swagger";
    private const string CorsKey = "CsLibWeb:Cors";
    private const string EndpointsKey = "CsLibWeb:Endpoints";
    private const string AuthKey = "CsLibWeb:Auth";
    private const string SerilogKey = "CsLibWeb:Serilog";

    private static void LogJson(string message, object? properties = null)
    {
        var logEntry = new Dictionary<string, object?>
        {
            ["@t"] = DateTime.UtcNow.ToString("O"),
            ["@m"] = message,
            ["SourceContext"] = "CsLibWeb"
        };

        if (properties != null)
        {
            foreach (var prop in properties.GetType().GetProperties())
            {
                logEntry[prop.Name] = prop.GetValue(properties);
            }
        }

        Console.WriteLine(JsonSerializer.Serialize(logEntry));
    }

    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Adds and configures Swagger documentation using FastEndpoints.
        /// </summary>
        /// <param name="args">Command line arguments to check for <c>--exportswaggerjson</c>.</param>
        /// <param name="name">The name of the Swagger document.</param>
        /// <param name="version">The version of the API.</param>
        /// <param name="title">The title of the API.</param>
        /// <returns>The <see cref="WebApplicationBuilder"/> instance.</returns>
        public WebApplicationBuilder AddSwagger(string[] args,
            string name,
            string version,
            string title
        )
        {
            var exportSwagger = args.Contains("--exportswaggerjson", StringComparer.OrdinalIgnoreCase);
            if (exportSwagger)
            {
                builder.WebHost.UseUrls("http://localhost:4999"); // export port
            }

            builder.Services.SwaggerDocument(d =>
            {
                d.ShortSchemaNames = true;
                d.DocumentSettings = settings =>
                {
                    settings.DocumentName = name;
                    settings.Version = version;
                    settings.Title = title;
                    settings.MarkNonNullablePropsAsRequired();
                    settings.PostProcess = s => { s.Servers.Add(new OpenApiServer { Url = "http://localhost:5203" }); };
                };
            });

            builder.Configuration[SwaggerKey] = "true";
            LogJson($"Swagger added: Name={name}, Version={version}, Title={title}, ExportMode={exportSwagger}",
                new { name, version, title, exportSwagger });
            return builder;
        }

        /// <summary>
        /// Adds and configures CORS (Cross-Origin Resource Sharing) based on configuration.
        /// </summary>
        /// <returns>The <see cref="WebApplicationBuilder"/> instance.</returns>
        public WebApplicationBuilder AddCors()
        {
            var corsOptions = builder.Configuration.GetSection(Cors.SectionName).Get<Cors>()!;
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy.WithOrigins(corsOptions.AllowedOrigins)
                    .WithMethods(corsOptions.AllowedMethods)
                    .WithHeaders(corsOptions.AllowedHeaders));
            });

            builder.Configuration[CorsKey] = "true";
            LogJson(
                $"CORS added: Origins=[{string.Join(',', corsOptions.AllowedOrigins)}] Methods=[{string.Join(',', corsOptions.AllowedMethods)}] Headers=[{string.Join(',', corsOptions.AllowedHeaders)}]",
                new
                {
                    Origins = corsOptions.AllowedOrigins, Methods = corsOptions.AllowedMethods,
                    Headers = corsOptions.AllowedHeaders
                });
            return builder;
        }

        /// <summary>
        /// Registers FastEndpoints with the provided discovered types and adds health checks.
        /// </summary>
        /// <param name="discoveredTypes">A list of types discovered by the source generator to be registered with FastEndpoints.</param>
        /// <returns>The <see cref="WebApplicationBuilder"/> instance.</returns>
        public WebApplicationBuilder AddEndpoints(List<Type> discoveredTypes)
        {
            builder.Services.AddFastEndpoints(o => { o.SourceGeneratorDiscoveredTypes.AddRange(discoveredTypes); })
                .AddHealthChecks();

            builder.Configuration[EndpointsKey] = "true";
            LogJson($"Endpoints added: DiscoveredTypeCount={discoveredTypes.Count}",
                new { DiscoveredTypeCount = discoveredTypes.Count });
            return builder;
        }

        /// <summary>
        /// Adds and configures Microsoft Identity Web API authentication and authorization.
        /// </summary>
        /// <returns>The <see cref="WebApplicationBuilder"/> instance.</returns>
        public WebApplicationBuilder AddAuth()
        {
            builder.Services.AddAuthentication()
                .AddMicrosoftIdentityWebApi(options =>
                    {
                        builder.Configuration.Bind("AzureAd", options);
                        options.TokenValidationParameters.NameClaimType = "name";
                    },
                    options => { builder.Configuration.Bind("AzureAd", options); });

            builder.Services.AddAuthorization();
            builder.Configuration[AuthKey] = "true";
            var clientId = builder.Configuration["AzureAd:ClientId"];
            LogJson($"Auth added: Mode=AzureAd, ClientId={clientId}",
                new { Mode = "AzureAd", ClientId = clientId });
            return builder;
        }

        /// <summary>
        /// Adds and configures Serilog for logging.
        /// </summary>
        /// <param name="configureLogger">An optional action to further configure the <see cref="LoggerConfiguration"/>.</param>
        /// <returns>The <see cref="WebApplicationBuilder"/> instance.</returns>
        public WebApplicationBuilder AddSerilog(Action<LoggerConfiguration>? configureLogger = null)
        {
            builder.Services.AddSerilog(c =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    c.WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level}] {RequestMethod} {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
                }
                else
                {
                    c.WriteTo.Console(new RenderedCompactJsonFormatter());
                }

                c
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure.CorsService",
                        LogEventLevel.Warning);

                c.Enrich.FromLogContext()
                    .Filter.ByExcluding(Matching
                        .WithProperty<string>("RequestMethod", p => p == "OPTIONS"));

                configureLogger?.Invoke(c);
            });
            builder.Configuration[SerilogKey] = "true";
            LogJson("Serilog added");
            return builder;
        }

        /// <summary>
        /// Builds the <see cref="WebApplication"/> and configures middleware conditionally based on Add* calls.
        /// </summary>
        /// <param name="binding">An optional action to configure <see cref="BindingOptions"/>, typically used to add source generated types.</param>
        /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
        public WebApplication BuildAndConfigureApp(Action<BindingOptions>? binding = null)
        {
            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CsLibWeb");

            CancellationTokenSource cancellation = new();
            app.Lifetime.ApplicationStopping.Register(() => { cancellation.Cancel(); });

            if (builder.Configuration[SerilogKey] == "true")
            {
                app.UseSerilogRequestLogging();
            }

            if (builder.Configuration[EndpointsKey] == "true")
            {
                logger.LogInformation("Enable FastEndpoints + health checks");
                app.MapHealthChecks("/healthz");
                app.UseCsLibExceptionHandler()
                    .UseFastEndpoints(c =>
                    {
                        c.Endpoints.ShortNames = true;
                        binding?.Invoke(c.Binding);
                        c.Serializer.Options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    });
            }

            if (builder.Configuration[SwaggerKey] == "true")
            {
                logger.LogInformation("Enable Swagger");
                app.UseSwaggerGen();
            }

            if (builder.Configuration[CorsKey] == "true")
            {
                logger.LogInformation("Enable CORS ({Mode})", app.Environment.IsDevelopment() ? "AllowAny" : "Policy");
                if (app.Environment.IsDevelopment())
                {
                    app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
                }
                else
                {
                    app.UseCors();
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                logger.LogInformation("Enable HSTS");
                app.UseHsts();
            }
            else
            {
                logger.LogDebug("HSTS skipped in development");
            }

            if (builder.Configuration[AuthKey] == "true")
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            return app;
        }
    }
}