# Grad.CsLib

*Generated from Grad.CsLib.xml*


## ClientErrorLog

**Type**: `Grad.CsLib.ClientErrorLogging.ClientErrorLog`

Represents a client-side error log entry.

---


## ClientErrorLoggingExtensions

**Type**: `Grad.CsLib.ClientErrorLogging.ClientErrorLoggingExtensions`

Provides extension methods for `Microsoft.AspNetCore.Builder.WebApplication` to add client-side error logging endpoints.

---


### MapClientErrorLogging(string)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplication`

**Method**: `Grad.CsLib.ClientErrorLogging.ClientErrorLoggingExtensions.MapClientErrorLogging(Microsoft.AspNetCore.Builder.WebApplication,string)`

Maps a client-side error logging endpoint to the specified path.
 The endpoint accepts POST requests with error type, message, stacktrace, and contextual data,
 and logs them to Serilog for collection by Splunk.

**path**: The path where the endpoint should be mapped (e.g., "/api/client-errors").

---


## ClientErrorLogValidator

**Type**: `Grad.CsLib.ClientErrorLogging.ClientErrorLogValidator`

Validator for `Grad.CsLib.ClientErrorLogging.ClientErrorLog` ensuring length limits and data constraints.

---


## CsLibWeb

**Type**: `Grad.CsLib.CsLibWeb`

Provides extension methods for `Microsoft.AspNetCore.Builder.WebApplicationBuilder` to simplify common web application setup
 based on Graduate College standards.

---


### AddSwagger(string[],string,string,string)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddSwagger(Microsoft.AspNetCore.Builder.WebApplicationBuilder,string[],string,string,string)`

Adds and configures Swagger documentation using FastEndpoints.

**args**: Command line arguments to check for `--exportswaggerjson`.

**name**: The name of the Swagger document.

**version**: The version of the API.

**title**: The title of the API.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddCors()

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddCors(Microsoft.AspNetCore.Builder.WebApplicationBuilder)`

Adds and configures CORS (Cross-Origin Resource Sharing) based on configuration.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddEndpoints(System.Collections.Generic.List<System.Type>)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddEndpoints(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Collections.Generic.List<System.Type>)`

Registers FastEndpoints with the provided discovered types and adds health checks.

**discoveredTypes**: A list of types discovered by the source generator to be registered with FastEndpoints.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddAuth()

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddAuth(Microsoft.AspNetCore.Builder.WebApplicationBuilder)`

Adds and configures Microsoft Identity Web API authentication and authorization.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddAuthAzureAdOrDevApiKey(string)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddAuthAzureAdOrDevApiKey(Microsoft.AspNetCore.Builder.WebApplicationBuilder,string)`

Adds a combined authentication scheme which uses a development API key if the `Api-Key` header is present;
 otherwise it falls back to Azure AD (Entra) bearer authentication.

**expectedApiKeyConfigKey**: Configuration key containing the expected API key value.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddSerilog(Action<Serilog.LoggerConfiguration>)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.AddSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,Action<Serilog.LoggerConfiguration>)`

Adds and configures Serilog for logging.

**configureLogger**: An optional action to further configure the `Serilog.LoggerConfiguration`.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### BuildAndConfigureApp(Action<FastEndpoints.BindingOptions>)

**Extends**: `Microsoft.AspNetCore.Builder.WebApplicationBuilder`

**Method**: `Grad.CsLib.CsLibWeb.BuildAndConfigureApp(Microsoft.AspNetCore.Builder.WebApplicationBuilder,Action<FastEndpoints.BindingOptions>)`

Builds the `Microsoft.AspNetCore.Builder.WebApplication` and configures middleware conditionally based on Add* calls.

**binding**: An optional action to configure `FastEndpoints.BindingOptions`, typically used to add source generated types.

**Returns**: The configured `Microsoft.AspNetCore.Builder.WebApplication` instance.

---


## ExceptionHandlerExtensions

**Type**: `Grad.CsLib.Errors.ExceptionHandlerExtensions`

extensions for global exception handling

---


## __LogUnStructuredExceptionStruct

**Type**: `Grad.CsLib.Errors.LoggingExtensions.__LogUnStructuredExceptionStruct`

This API supports the logging infrastructure and is not intended to be used directly from your code. It is subject to change in the future.

---


## GradValidators

**Type**: `Grad.CsLib.Validators.GradValidators`

Provides custom validation rules for specific use cases in Graduate College applications.

---


### MustBeProgramCode()

**Extends**: `FluentValidation.IRuleBuilder<T,string>`

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeProgramCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid program code.

---


### MustBeDepartmentCode()

**Extends**: `FluentValidation.IRuleBuilder<T,string>`

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeDepartmentCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid department code.

---


### MustBeTermCode()

**Extends**: `FluentValidation.IRuleBuilder<T,string>`

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeTermCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid term code, e.g. 120248.

---


### MustBeUin()

**Extends**: `FluentValidation.IRuleBuilder<T,string>`

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeUin(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid 9-digit numeric UIN (University Identification Number).

---


### MustBeValidDbIdentifier()

**Extends**: `FluentValidation.IRuleBuilder<T,string>`

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeValidDbIdentifier(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid database identifier.

---
