# CsLib.Data

This package has utilities for EF Core and Dapper.

I prefer EF Core's scaffold tool, but it is painful to write queries
with, so for that we use Dapper.

This project is intended entirely for SQL Server.

## Requirements

- .NET Core 10
- dotnet-ef: `dotnet tool install --global dotnet-ef`

## Dependencies

Here are the main dependencies:

- [Dapper](https://github.com/DapperLib/Dapper)
- [DbUp](https://dbup.readthedocs.io/en/latest/)
- [Microsoft.EntityFrameworkCore.Design](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet)

## Schema Migrations

Schemas are intended to be written in SQL files and then scaffolded with EF Core into
C# models and DbContext classes.

The convention is:

1. Have a project in the solution which name ends in `Data`, e.g. `ExampleData`.
2. Have a folder inside it called `schema`, containing SQL files that sort correctly to run in order.
   - For example, `0001_Initial.sql`, `0002_AddTable.sql`, etc.
   - Include them in the assembly:
     ```xml
     <ItemGroup>
       <EmbeddedResource Include="schema/**/*.sql" />
     </ItemGroup>
    ```
3. In the `Data` project's `Program.cs`, create a `DatabaseMigrator` and call its `Upgrade()` method.
4. Run the `Data` project against the database.
5. Run the EF Core scaffold tool against the database.

The [CsLib.Tool](https://www.nuget.org/packages/Grad.CsLib.Tool/) project has some automation for this flow,
it will take care of running the migrations and scaffolding the models against a temporary Docker database.

## Other Utilities

See the [Grad.CsLib.Data.md](./Grad.CsLib.Data.md) file for the full list.
