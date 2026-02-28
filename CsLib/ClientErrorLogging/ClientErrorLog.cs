using System.Text.Json.Serialization;

namespace Grad.CsLib.ClientErrorLogging;

/// <summary>
/// Represents a client-side error log entry.
/// </summary>
public class ClientErrorLog
{
    /// <summary>
    /// The type or category of the error (e.g., "TypeError", "NetworkError").
    /// Maximum length: 100 characters.
    /// </summary>
    [JsonPropertyName("errorType")]
    public required string ErrorType { get; init; }

    /// <summary>
    /// The error message describing what went wrong.
    /// Maximum length: 1000 characters.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// The stack trace of the error, if available.
    /// Maximum length: 5000 characters.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; init; }

    /// <summary>
    /// Additional contextual data as key-value pairs.
    /// Keys must be strings, values must be primitives (string, number, boolean, null).
    /// Maximum 20 entries, each key max 50 chars, each value max 500 chars.
    /// </summary>
    [JsonPropertyName("context")]
    public Dictionary<string, object?>? Context { get; init; }
}
