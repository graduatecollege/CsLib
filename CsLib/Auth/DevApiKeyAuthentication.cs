using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grad.CsLib.Auth;

public static class DevApiKeyDefaults
{
    public const string AuthenticationScheme = "DevApiKey";
    public const string DefaultHeaderName = "Api-Key";
}

public sealed class DevApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = DevApiKeyDefaults.DefaultHeaderName;

    /// <summary>
    /// The expected API key value. If null/empty, authentication will fail when the header is present.
    /// </summary>
    public string? ExpectedApiKey { get; set; }
}

public sealed class DevApiKeyAuthenticationHandler : AuthenticationHandler<DevApiKeyAuthenticationOptions>
{
    public DevApiKeyAuthenticationHandler(
        IOptionsMonitor<DevApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var providedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrWhiteSpace(Options.ExpectedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"Dev API key auth is enabled but no expected key is configured (Options.{nameof(DevApiKeyAuthenticationOptions.ExpectedApiKey)} is empty)."));
        }

        if (!string.Equals(providedKey.ToString(), Options.ExpectedApiKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }
        
        var roleValue = Request.Headers["Api-Role"].FirstOrDefault();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-api-key"),
            new Claim(ClaimTypes.Name, "Dev Api Key"),
            new Claim(ClaimTypes.Role, roleValue ?? "Developer")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
