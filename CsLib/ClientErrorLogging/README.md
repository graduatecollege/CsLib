# Client-Side Error Logging

The `ClientErrorLogging` namespace provides a convenient way to add a client-side error logging endpoint to your ASP.NET Core application. This allows frontend applications to send error information to the backend, where it's logged to Serilog for collection and analysis by Splunk.

## Features

- **Simple integration**: Single extension method to add the endpoint
- **Validation**: Built-in validation for error data with configurable limits
- **Structured logging**: Errors are logged to Serilog with structured data
- **Anonymous access**: No authentication required (useful for client errors)
- **Flexible routing**: Map to any path you choose

## Usage

### 1. Add the endpoint to your application

After building your `WebApplication` with `CsLibWeb.BuildAndConfigureApp()`, map the client error logging endpoint:

```csharp
using Grad.CsLib;
using Grad.CsLib.ClientErrorLogging;

var builder = WebApplication.CreateBuilder(args);

// Configure services...
builder.AddSerilog()
    .AddEndpoints(DiscoveredTypes.All)
    .AddCors();

var app = builder.BuildAndConfigureApp();

// Map the client error logging endpoint
app.MapClientErrorLogging("/api/client-errors");

app.Run();
```

### 2. Send errors from your client application

Send a POST request to the configured endpoint with a JSON payload:

```javascript
// JavaScript/TypeScript example
const logClientError = async (error) => {
    try {
        await fetch('/api/client-errors', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                errorType: error.name || 'Error',
                message: error.message,
                stackTrace: error.stack,
                context: {
                    url: window.location.href,
                    userAgent: navigator.userAgent,
                    timestamp: new Date().toISOString()
                }
            })
        });
    } catch (e) {
        // Fail silently to avoid recursive errors
        console.error('Failed to log error to server', e);
    }
};

// Use in error handlers
window.addEventListener('error', (event) => {
    logClientError(event.error);
});

// Or in promise rejections
window.addEventListener('unhandledrejection', (event) => {
    logClientError(event.reason);
});
```

## Request Schema

```json
{
    "errorType": "string (required, max 100 chars)",
    "message": "string (required, max 1000 chars)",
    "stackTrace": "string (optional, max 5000 chars)",
    "context": {
        "key1": "value1",
        "key2": 123,
        "key3": true
    }
}
```

### Validation Rules

- **errorType**: Required, maximum 100 characters
- **message**: Required, maximum 1000 characters
- **stackTrace**: Optional, maximum 5000 characters
- **context**: Optional, with the following constraints:
  - Maximum 20 key-value pairs
  - Keys: Maximum 50 characters each
  - Values: Must be primitives (string, number, boolean, null)
  - String values: Maximum 500 characters each

If the client-side data exceeds these limits, truncate the strings before sending.

## Response

### Success (200 OK)
```json
{}
```

### Validation Error (400 Bad Request)
```json
{
    "error": "ValidationFailed",
    "errors": [
        {
            "propertyName": "Message",
            "errorMessage": "Message must not exceed 1000 characters"
        }
    ]
}
```

## Log Output

Errors are logged with the following structured format:

```
Client error: Type={ErrorType}, Message={Message}, StackTrace={StackTrace}, Context={@Context}
```

In production (non-development environments), logs are output in JSON format for Splunk ingestion. In development, they use a human-readable format.

## Example Integration

```csharp
// Program.cs
using Grad.CsLib;
using Grad.CsLib.ClientErrorLogging;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddSerilog()
    .AddSwagger(args, "MyApp", "v1", "My Application API")
    .AddEndpoints(DiscoveredTypes.All)
    .AddCors()
    .AddAuth();

var app = builder.BuildAndConfigureApp();

// Add client error logging
app.MapClientErrorLogging("/api/client-errors");

app.Run();
```

## Security Considerations

- The endpoint allows anonymous access to enable error logging even when users are not authenticated
- Input validation prevents oversized payloads
- Only primitive types are allowed in context data to prevent complex object serialization issues
- Rate limiting should be applied at the infrastructure level (e.g., API Gateway) to prevent abuse
