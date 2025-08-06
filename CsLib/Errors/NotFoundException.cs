namespace Grad.CsLib.Errors;

public class NotFoundException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}