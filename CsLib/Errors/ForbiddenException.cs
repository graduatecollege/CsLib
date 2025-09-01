namespace Grad.CsLib.Errors;

public class ForbiddenException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}