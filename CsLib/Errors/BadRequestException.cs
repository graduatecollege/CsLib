namespace Grad.CsLib.Errors;

public class BadRequestException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}