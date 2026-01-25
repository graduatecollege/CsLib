# AGENTS.md

This document provides a quick overview of the CsLib repository for AI coding agents.

## Project Overview

CsLib is a common library for Graduate College web applications and APIs at the University of Illinois. it provides
boilerplate, common utilities, and standardized patterns for .NET-based services.

## Tech Stack

- **Framework:** .NET 10.0
- **Language:** C# 14.0 (utilizing latest features and experimental extension syntax)
- **Web Framework:** [FastEndpoints](https://fast-endpoints.com/) (a minimal wrapper around .NET Core minimal APIs)
- **Database Migrations:** [DbUp](https://dbup.github.io/)
- **Validation:** [FluentValidation](https://docs.fluentvalidation.net/)
- **Logging:** Serilog
- **Documentation:** Swagger/NSwag
- **Tooling:** `cslib` (dotnet tool for EF scaffolding and OpenAPI generation)

## Project Structure

- `/CsLib`: Core library for web application APIs. Examples of contents:
    - `/Errors`: Custom exceptions (`BadRequestException`, `NotFoundException`, etc.) and a global `ExceptionHandler`.
    - `/Options`: Configuration objects (e.g., `CorsOptions`).
    - `/Validators`: Common FluentValidation extensions for University-specific codes (UINs, Term codes, etc.).
    - `CsLibWeb.cs`: Main entry point for configuring `WebApplicationBuilder` with standard middleware (Swagger, Auth,
      CORS, Serilog, FastEndpoints).
- `/CsLib.Data`: Common library for database functionality. Examples of contents:
    - `DatabaseMigrator.cs`: Wrapper for DbUp migrations.
    - `QueryableExtensions.cs`: Helpers for sorting, paging, and filtering `IQueryable` sources.
    - `PageOptions.cs`: Standard DTO for pagination and sorting parameters.
- `/CsLib.Tool`: Source for the `cslib` dotnet tool.
    - Used for EF Core scaffolding (`models`, `rebuild-models`) and OpenAPI/TypeScript generation (`openapi`).
    - Depends on specific project naming conventions (`*Data.csproj`, `*Server.csproj`).

## Code conventions

- Avoid obvious comments when the code is self-explanatory.