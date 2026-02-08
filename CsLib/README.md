# CsLib Core Package

This package contains utilities for C# API projects.

This package is very opinionated, as it's intended for Graduate College API projects:

- FastEndpoints is used, which is a thin layer on top of .NET Core minimal APIs.
- OpenAPI/Swagger docs are generated from endpoint definitions and XML documentation.
  The JSON is exported to the `/spec` folder for generating client libraries.
    - [Kiota](https://github.com/microsoft/kiota) is used to generate the OpenAPI JSON,
      but not the client libraries.
- Authentication is through Entra.

## Requirements

- .NET Core 10

## Dependencies

Outside of Microsoft packages, these are the main dependencies:

- [FastEndpoints](https://fast-endpoints.com/)
- [Mapperly](https://mapperly.riok.app/)
- [Serilog](https://serilog.net/)

## `CsLibWeb`

Here's an example of how to set up a `Program.cs` file for a web API project:

```csharp
using ExampleData;
using ExampleServer;
using FastEndpoints.ClientGen.Kiota;
using Grad.CsLib;
using Grad.CsLib.ClientErrorLogging;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSwagger(args, "ExampleServer", "0.0.1", "Example Backend Server");

builder
    .AddSerilog()
    .AddCors()
    .AddAuth();
builder.Services
    .AddDbContext<ExampleContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("Example")))
    .AddAuthorization(AuthPolicy.AddAuthPolicy)
    .RegisterServicesFromExampleServer();

builder.AddEndpoints(DiscoveredTypes.All);

try
{
    var app = builder.BuildAndConfigureApp(b => b.ReflectionCache.AddFromExampleData().AddFromExampleServer());

    // Optional: Add client-side error logging endpoint
    app.MapClientErrorLogging("/api/client-errors");

    // This is only run with the --exportswaggerjson option
    await app.ExportSwaggerJsonAndExitAsync("ExampleServer", "../spec");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
```

## Client-Side Error Logging

CsLib provides a convenient endpoint for logging client-side errors. This allows frontend applications to send error information to the backend for centralized logging and analysis.

For detailed documentation, see [ClientErrorLogging/README.md](./ClientErrorLogging/README.md).

Quick example:

```csharp
// In Program.cs, after building the app
app.MapClientErrorLogging("/api/client-errors");
```

This creates an anonymous POST endpoint that accepts client errors with validation for error type, message, stack trace, and contextual data. All errors are logged to Serilog for collection by Splunk.

