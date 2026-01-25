using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;

var rootCommand = new RootCommand("CsLib management tool");

var modelsCommand =
    new Command("models", "Scaffold the database context and entity classes using Entity Framework Core.");
var connectionStringOption =
    new Option<string?>("--connection-string") { Description = "The connection string to use." };
connectionStringOption.Aliases.Add("-c");
modelsCommand.Options.Add(connectionStringOption);

modelsCommand.SetAction(async parseResult =>
{
    var connectionString = parseResult.GetValue(connectionStringOption);
    await HandleModels(connectionString);
});

var rebuildModelsCommand = new Command("rebuild-models", "Rebuild models using a temporary SQL Server container.");
rebuildModelsCommand.SetAction(async _ => { await HandleRebuildModels(); });

var openapiCommand = new Command("openapi", "Builds the OpenAPI JSON spec and TypeScript client files.");
openapiCommand.SetAction(async _ => { await HandleOpenApi(); });

rootCommand.Subcommands.Add(modelsCommand);
rootCommand.Subcommands.Add(rebuildModelsCommand);
rootCommand.Subcommands.Add(openapiCommand);

if (args.Length == 0)
{
    rootCommand.Parse("-h").Invoke();
    Console.WriteLine();
    Console.WriteLine("Project Structure Requirements:");
    Console.WriteLine("  This tool expects a specific project structure to function correctly:");
    Console.WriteLine("  - Data Project: A project file ending in 'Data.csproj' (e.g., MyAppData.csproj).");
    Console.WriteLine("    Used by: 'models', 'rebuild-models'.");
    Console.WriteLine("  - Server Project: A project file ending in 'Server.csproj' (e.g., MyAppServer.csproj).");
    Console.WriteLine("    Used by: 'openapi'.");
    Console.WriteLine("  - spec/ directory: A directory named 'spec' in the current working directory.");
    Console.WriteLine("    Used by: 'openapi' to run 'npm run build'.");
    Console.WriteLine("  - .connection-string file: A file in the current directory containing the base connection string.");
    Console.WriteLine("    Used by: 'models' (if --connection-string is not provided).");
    return 0;
}

var parseResult = rootCommand.Parse(args);
return parseResult.Invoke();

async Task HandleModels(string? connectionString)
{
    string fullConnection;
    if (!string.IsNullOrEmpty(connectionString))
    {
        fullConnection = connectionString;
    }
    else
    {
        var connectionFile = Path.Combine(Directory.GetCurrentDirectory(), ".connection-string");
        if (!File.Exists(connectionFile))
        {
            Console.WriteLine($"Error: Connection string file not found at {connectionFile}");
            return;
        }

        var baseConnection = string.Join("",
            File.ReadLines(connectionFile)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .Select(l => l.EndsWith(";") ? l : l + ";"));

        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        if (string.IsNullOrEmpty(password))
        {
            password = GetPasswordFromKeyring("certification", "database");
            if (string.IsNullOrEmpty(password))
            {
                Console.Write("Password not found in keyring. Please enter it: ");
                password = ReadPassword();
                Console.WriteLine();
                SetPasswordInKeyring("certification", "database", password);
            }
        }

        fullConnection = $"{baseConnection}Password={password};";
    }

    var dataProject = FindProject("Data");
    if (dataProject == null)
    {
        Console.WriteLine("Error: Could not find a project ending in 'Data'.");
        return;
    }

    var projectDir = Path.GetDirectoryName(dataProject)!;

    await RunProcess("dotnet",
        $"ef dbcontext scaffold \"{fullConnection}\" Microsoft.EntityFrameworkCore.SqlServer --schema dbo --output-dir models --context-dir . --force --no-onconfiguring",
        projectDir);
}

async Task HandleRebuildModels()
{
    const string containerName = "sqlserver_rebuild_models";
    const string saPassword = "Password123!";
    const int port = 14335;
    const string dbName = "Certification";
    var containerConnStr =
        $"Server=localhost,{port};Database={dbName};User Id=sa;Password={saPassword};TrustServerCertificate=True";

    try
    {
        Console.WriteLine("Starting SQL Server container...");
        await RunProcess("docker",
            $"run -e \"ACCEPT_EULA=Y\" -e \"MSSQL_SA_PASSWORD={saPassword}\" -p \"{port}:1433\" --name \"{containerName}\" -d mcr.microsoft.com/mssql/server:2022-latest");

        Console.WriteLine("Waiting for SQL Server to be ready...");
        await Task.Delay(5000);

        Console.WriteLine($"Creating database {dbName}");
        await RunProcess("docker",
            $"exec {containerName} /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"{saPassword}\" -Q \"CREATE DATABASE [{dbName}]\"");

        Console.WriteLine($"Creating schema [{dbName}].DbUp");
        await RunProcess("docker",
            $"exec {containerName} /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P \"{saPassword}\" -d \"{dbName}\" -Q \"CREATE SCHEMA DbUp\"");

        Console.WriteLine("Migrating schema...");
        var dataProject = FindProject("Data");
        if (dataProject == null)
        {
            Console.WriteLine("Error: Could not find a project ending in 'Data'.");
            return;
        }

        await RunProcess("dotnet", $"run --project \"{dataProject}\" \"{containerConnStr}\"");

        Console.WriteLine("Regenerating EF models...");
        await HandleModels(containerConnStr);

        Console.WriteLine("Done!");
    }
    finally
    {
        Console.WriteLine("Cleaning up...");
        await RunProcess("docker", $"stop {containerName}", silent: true);
        await RunProcess("docker", $"rm {containerName}", silent: true);
    }
}

async Task HandleOpenApi()
{
    var serverProject = FindProject("Server");
    if (serverProject == null)
    {
        Console.WriteLine("Error: Could not find a project ending in 'Server'.");
        return;
    }

    var serverDir = Path.GetDirectoryName(serverProject)!;
    Console.WriteLine($"Building OpenAPI for {serverProject}...");
    await RunProcess("dotnet", "run -c Release --exportswaggerjson true", serverDir);

    var specDir = Path.Combine(Directory.GetCurrentDirectory(), "spec");
    if (Directory.Exists(specDir))
    {
        Console.WriteLine("Building TypeScript client...");
        await RunProcess("npm", "run build", specDir);
    }
    else
    {
        Console.WriteLine("Warning: 'spec' directory not found, skipping npm build.");
    }
}

string? FindProject(string suffix)
{
    var rootDir = Directory.GetCurrentDirectory();
    return Directory.EnumerateFiles(rootDir, $"*{suffix}.csproj", SearchOption.AllDirectories)
        .FirstOrDefault();
}

async Task RunProcess(string command, string arguments, string? workingDirectory = null, bool silent = false)
{
    var psi = new ProcessStartInfo
    {
        FileName = command,
        Arguments = arguments,
        WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
        RedirectStandardOutput = silent,
        RedirectStandardError = silent,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = Process.Start(psi);
    if (process == null)
    {
        return;
    }

    await process.WaitForExitAsync();
}

string GetPasswordFromKeyring(string service, string account)
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return "";
    }

    if (!EnsurePythonAndKeyringInstalled())
    {
        return "";
    }

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"-c \"import keyring; print(keyring.get_password('{service}', '{account}') or '')\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null)
        {
            return "";
        }

        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return output;
    }
    catch
    {
        return "";
    }
}

void SetPasswordInKeyring(string service, string account, string password)
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return;
    }

    if (!EnsurePythonAndKeyringInstalled())
    {
        return;
    }

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"-c \"import keyring; keyring.set_password('{service}', '{account}', '{password}')\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        process?.WaitForExit();
    }
    catch
    {
        /* Ignore */
    }
}

bool EnsurePythonAndKeyringInstalled()
{
    try
    {
        var pythonCheck = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var pythonProcess = Process.Start(pythonCheck);
        if (pythonProcess == null)
        {
            throw new Exception("Failed to start python3");
        }

        pythonProcess.WaitForExit();
        if (pythonProcess.ExitCode != 0)
        {
            throw new Exception("python3 returned non-zero exit code");
        }
    }
    catch
    {
        Console.WriteLine("Error: 'python3' is not installed or not in PATH.");
        Console.WriteLine("Please install Python 3 to use the keyring integration.");
        Console.WriteLine("On Ubuntu/Debian: sudo apt install python3");
        return false;
    }

    try
    {
        var keyringCheck = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = "-c \"import keyring\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var keyringProcess = Process.Start(keyringCheck);
        if (keyringProcess == null)
        {
            throw new Exception("Failed to start python3 for keyring check");
        }

        keyringProcess.WaitForExit();
        if (keyringProcess.ExitCode != 0)
        {
            throw new Exception("keyring package not found");
        }
    }
    catch
    {
        Console.WriteLine("Error: Python 'keyring' package is not installed.");
        Console.WriteLine("Please install it using pip:");
        Console.WriteLine("python3 -m pip install keyring");
        return false;
    }

    return true;
}

string ReadPassword()
{
    var password = string.Empty;
    ConsoleKeyInfo key;
    do
    {
        key = Console.ReadKey(true);
        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password += key.KeyChar;
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[..^1];
        }
    } while (key.Key != ConsoleKey.Enter);

    return password;
}