namespace Grad.CsLib.Data;

/// <summary>
/// Represents an exception thrown when an invalid parameter is encountered.
/// </summary>
public class InvalidParameterException : Exception
{
    public InvalidParameterException(string message) : base(message) { }
    public InvalidParameterException(string message, Exception innerException) : base(message, innerException) { }
}