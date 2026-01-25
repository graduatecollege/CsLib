# CsLib

Graduate College web application common library for web application APIs
and other related projects.

## Requirements

- .NET Core 10
- CsLib depends on [FastEndpoints](https://fast-endpoints.com/), a minimal wrapper around .NET Core minimal APIs.

## Project Structure

- `CsLib/` is primarily for API development.
- `CsLib.Data/` is primarily for database functionality.
- `CsLib.Tool/` contains the `cslib` dotnet tool for project management tasks.

## CsLib Tool (`cslib`)

`cslib` is a dotnet tool designed to automate common development tasks like database model generation and OpenAPI spec updates.

### Installation

To install the tool globally:

```bash
dotnet tool install -g Grad.CsLib.Tool
```

### Commands

- `cslib models`: Scaffolds EF Core database context and entity classes.
- `cslib rebuild-models`: Runs migrations in a temporary Docker container and regenerates models.
- `cslib openapi`: Exports Swagger/OpenAPI JSON and builds the TypeScript client.

### Project Requirements

The tool expects the following structure in the project it is run from:
- A `*Data.csproj` project (e.g., `MyProject.Data.csproj`).
- A `*Server.csproj` project (e.g., `MyProject.Server.csproj`).
- A `spec/` directory (for `openapi` command).
- A `.connection-string` file (for `models` command).
