# Grad.CsLib

*Generated from Grad.CsLib.xml*


## CsLibWeb

**Type**: `Grad.CsLib.CsLibWeb`

Provides extension methods for `Microsoft.AspNetCore.Builder.WebApplicationBuilder` to simplify common web application setup
 based on Graduate College standards.

---


### AddSwagger(Microsoft.AspNetCore.Builder.WebApplicationBuilder,string[],string,string,string)

**Method**: `Grad.CsLib.CsLibWeb.AddSwagger(Microsoft.AspNetCore.Builder.WebApplicationBuilder,string[],string,string,string)`

Adds and configures Swagger documentation using FastEndpoints.

**args**: Command line arguments to check for `--exportswaggerjson`.

**name**: The name of the Swagger document.

**version**: The version of the API.

**title**: The title of the API.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddCors(Microsoft.AspNetCore.Builder.WebApplicationBuilder)

**Method**: `Grad.CsLib.CsLibWeb.AddCors(Microsoft.AspNetCore.Builder.WebApplicationBuilder)`

Adds and configures CORS (Cross-Origin Resource Sharing) based on configuration.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddEndpoints(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Collections.Generic.List<System.Type>)

**Method**: `Grad.CsLib.CsLibWeb.AddEndpoints(Microsoft.AspNetCore.Builder.WebApplicationBuilder,System.Collections.Generic.List<System.Type>)`

Registers FastEndpoints with the provided discovered types and adds health checks.

**discoveredTypes**: A list of types discovered by the source generator to be registered with FastEndpoints.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddAuth(Microsoft.AspNetCore.Builder.WebApplicationBuilder)

**Method**: `Grad.CsLib.CsLibWeb.AddAuth(Microsoft.AspNetCore.Builder.WebApplicationBuilder)`

Adds and configures Microsoft Identity Web API authentication and authorization.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### AddSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,Action<Serilog.LoggerConfiguration>)

**Method**: `Grad.CsLib.CsLibWeb.AddSerilog(Microsoft.AspNetCore.Builder.WebApplicationBuilder,Action<Serilog.LoggerConfiguration>)`

Adds and configures Serilog for logging.

**configureLogger**: An optional action to further configure the `Serilog.LoggerConfiguration`.

**Returns**: The `Microsoft.AspNetCore.Builder.WebApplicationBuilder` instance.

---


### BuildAndConfigureApp(Microsoft.AspNetCore.Builder.WebApplicationBuilder,Action<FastEndpoints.BindingOptions>)

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


### MustBeProgramCode(FluentValidation.IRuleBuilder<T,string>)

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeProgramCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid program code.

---


### MustBeDepartmentCode(FluentValidation.IRuleBuilder<T,string>)

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeDepartmentCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid department code.

---


### MustBeTermCode(FluentValidation.IRuleBuilder<T,string>)

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeTermCode(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid term code, e.g. 120248.

---


### MustBeUin(FluentValidation.IRuleBuilder<T,string>)

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeUin(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid 9-digit numeric UIN (University Identification Number).

---


### MustBeValidDbIdentifier(FluentValidation.IRuleBuilder<T,string>)

**Method**: `Grad.CsLib.Validators.GradValidators.MustBeValidDbIdentifier(FluentValidation.IRuleBuilder<T,string>)`

Validates that a string is a valid database identifier.

---
