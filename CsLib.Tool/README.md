# CsLib.Tool

`cslib` is a dotnet tool designed to automate common development tasks
like database model generation and OpenAPI spec updates in Graduate College projects.

### Installation

Install the tool locally:

```bash
dotnet tool install --local Grad.CsLib.Tool
```

### Commands

- `dotnet tool run cslib -- rebuild-models`: Runs migrations in a temporary Docker container and regenerates models.
- `dotnet tool run cslib -- openapi`: Exports Swagger/OpenAPI JSON and builds the TypeScript client.

### Project Requirements

The tool expects the following structure in the project it is run from:

- A `*Data.csproj` project (e.g., `MyProject.Data.csproj`).
- A `*Server.csproj` project (e.g., `MyProject.Server.csproj`).
- A `spec/` directory (for `openapi` command).
- A `.connection-string` file (for `rebuild-models` command).

The `.connection-string` file should contain connection string elements with line breaks
instead of semicolons. For example:

```
Server=example.org
Database=Example
Encrypt=True
```
