# CsLib

Graduate College C# common library for web application APIs
and other related projects.

For details, see the package READMEs:

- [Grad.CsLib](./CsLib/README.md)
- [Grad.CsLib.Data](./CsLib.Data/README.md)
- [Grad.CsLib.Tool](./CsLib.Tool/README.md)

## Requirements

- .NET Core 10
- dotnet-ef: `dotnet tool install --global dotnet-ef`

## CsLib Tool (`cslib`)

`cslib` is a dotnet tool designed to automate common development tasks like database model generation and OpenAPI spec updates.

### Installation

Install the tool locally:

```bash
dotnet tool install --local Grad.CsLib.Tool
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
