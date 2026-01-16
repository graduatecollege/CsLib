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

namespace Grad.CsLib;

public static class CsLibWeb
{
    private const string SwaggerKey = "CsLibWeb:Swagger";
    private const string CorsKey = "CsLibWeb:Cors";
    private const string EndpointsKey = "CsLibWeb:Endpoints";
    private const string AuthKey = "CsLibWeb:Auth";

    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddSwagger(string[] args, string name,
            string version, string title)
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
                    settings.PostProcess = s =>
                    {
                        s.Servers.Add(new OpenApiServer { Url = "http://localhost:5203" });
                    };
                };
            });

            builder.Configuration[SwaggerKey] = "true";
            Console.WriteLine($"[CsLibWeb] Swagger added: Name={name}, Version={version}, Title={title}, ExportMode={exportSwagger}");
            return builder;
        }

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
            Console.WriteLine($"[CsLibWeb] CORS added: Origins=[{string.Join(',', corsOptions.AllowedOrigins)}] Methods=[{string.Join(',', corsOptions.AllowedMethods)}] Headers=[{string.Join(',', corsOptions.AllowedHeaders)}]");
            return builder;
        }

        public WebApplicationBuilder AddEndpoints(List<Type> discoveredTypes)
        {
            builder.Services.AddFastEndpoints(o => { o.SourceGeneratorDiscoveredTypes.AddRange(discoveredTypes); })
                .AddHealthChecks();

            builder.Configuration[EndpointsKey] = "true";
            Console.WriteLine($"[CsLibWeb] Endpoints added: DiscoveredTypeCount={discoveredTypes.Count}");
            return builder;
        }

        public WebApplicationBuilder AddAuth()
        {
            builder.Services.AddAuthentication()
                .AddMicrosoftIdentityWebApi(options =>
                {
                    builder.Configuration.Bind("AzureAd", options);
                    options.TokenValidationParameters.NameClaimType = "name";
                }, options => { builder.Configuration.Bind("AzureAd", options); });
        
            builder.Services.AddAuthorization();
            builder.Configuration[AuthKey] = "true";
            Console.WriteLine($"[CsLibWeb] Auth added: Mode=AzureAd, ClientId={builder.Configuration["AzureAd:ClientId"]}");
            return builder;
        }

        /// <summary>
        /// APIs that don't require authentication still need some form of authentication. This adds an insecure
        /// JWT Bearer authentication scheme.
        /// </summary>
        public WebApplicationBuilder AddNoAuth()
        {
            builder.Services
                .AddAuthenticationJwtBearer(s => s.SigningKey = "this is totally insecure")
                .AddAuthorization();

            builder.Configuration[AuthKey] = "true";
            Console.WriteLine("[CsLibWeb] Auth added: Mode=NoAuth (insecure dev JWT)");
            return builder;
        }

        /// <summary>
        /// Builds the WebApplication and configures middleware conditionally based on Add* calls.
        ///
        /// Accepts an optional function to configure BindingOptions, which you would typically
        /// use to add source generated types.
        /// </summary>
        public WebApplication BuildAndConfigureApp(Action<BindingOptions>? binding = null)
        {
            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CsLibWeb");

            CancellationTokenSource cancellation = new();
            app.Lifetime.ApplicationStopping.Register(() => { cancellation.Cancel(); });

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
                logger.LogInformation("Enable authentication/authorization");
                if (app.Environment.IsDevelopment())
                {
                    app.Use(async (context, next) =>
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith("Bearer developer", StringComparison.OrdinalIgnoreCase))
                        {
                            var claims = new List<System.Security.Claims.Claim>
                            {
                                new(System.Security.Claims.ClaimTypes.Name, "Developer"),
                                new(System.Security.Claims.ClaimTypes.Role, "Admin")
                            };
                            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(claims, "DeveloperToken"));
                            logger.LogDebug("Injected developer principal");
                        }
                        await next();
                    });
                }
                app.UseAuthentication();
                app.UseAuthorization();
            }

            return app;
        }
    }
}