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

namespace Grad.CsLib;

public static class CsLibWeb
{
    private const string SwaggerKey = "CsLibWeb:Swagger";
    private const string CorsKey = "CsLibWeb:Cors";
    private const string EndpointsKey = "CsLibWeb:Endpoints";
    private const string AuthKey = "CsLibWeb:Auth";

    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder, string[] args, string name,
        string version, string title)
    {
        var exportSwagger = args.Contains("--exportswaggerjson", StringComparer.OrdinalIgnoreCase);
        if (exportSwagger)
        {
            builder.WebHost.UseUrls("http://localhost:4999"); // Use a different port for export
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
                    s.Servers.Add(new OpenApiServer
                    {
                        Url = "http://localhost:5203"
                    });
                };
            };
        });

        builder.Configuration[SwaggerKey] = "true";
        return builder;
    }

    public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            var corsOptions = builder.Configuration.GetSection(Cors.SectionName).Get<Cors>()!;
            options.AddDefaultPolicy(policy => policy.WithOrigins(corsOptions.AllowedOrigins)
                .WithMethods(corsOptions.AllowedMethods)
                .WithHeaders(corsOptions.AllowedHeaders)
            );
        });

        builder.Configuration[CorsKey] = "true";
        return builder;
    }

    public static WebApplicationBuilder AddEndpoints(this WebApplicationBuilder builder, List<Type> discoveredTypes)
    {
        builder.Services.AddFastEndpoints(o =>
            {
                o.SourceGeneratorDiscoveredTypes.AddRange(discoveredTypes);
            })
            .AddHealthChecks();

        builder.Configuration[EndpointsKey] = "true";
        return builder;
    }

    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApi(options =>
            {
                builder.Configuration.Bind("AzureAd", options);
                options.TokenValidationParameters.NameClaimType = "name";
            }, options => { builder.Configuration.Bind("AzureAd", options); });
        
        builder.Services.AddAuthorization();
        builder.Configuration[AuthKey] = "true";

        return builder;
    }

    /// <summary>
    /// APIs that don't require authentication still need some form of authentication. This adds an insecure
    /// JWT Bearer authentication scheme.
    /// </summary>
    public static WebApplicationBuilder AddNoAuth(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthenticationJwtBearer(s => s.SigningKey = "this is totally insecure")
            .AddAuthorization();

        builder.Configuration[AuthKey] = "true";

        return builder;
    }

    /// <summary>
    /// Builds the WebApplication and configures middleware conditionally based on Add* calls.
    ///
    /// Accepts an optional function to configure BindingOptions, which you would typically
    /// use to add source generated types.
    /// </summary>
    public static WebApplication BuildAndConfigureApp(this WebApplicationBuilder builder, Action<BindingOptions>? binding = null)
    {
        var app = builder.Build();
        
        CancellationTokenSource cancellation = new();
        app.Lifetime.ApplicationStopping.Register(() => { cancellation.Cancel(); });

        if (builder.Configuration[EndpointsKey] == "true")
        {
            app.MapHealthChecks("/healthz");
            app.UseFastEndpoints(c =>
            {
                c.Endpoints.ShortNames = true;
                binding?.Invoke(c.Binding);
                c.Serializer.Options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                c.Endpoints.Configurator = ep =>
                {
                    ep.PostProcessor<ExceptionProcessor>(Order.After);
                };
            });
        }

        if (builder.Configuration[SwaggerKey] == "true")
        {
            app.UseSwaggerGen();
        }

        if (builder.Configuration[CorsKey] == "true")
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseCors(b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            }
            else
            {
                app.UseCors();
            }
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }
        
        if (builder.Configuration[AuthKey] == "true")
        {
            if (app.Environment.IsDevelopment())
            {
                // Custom middleware for developer token
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
                        var identity = new System.Security.Claims.ClaimsIdentity(claims, "DeveloperToken");
                        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                        context.User = principal;
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