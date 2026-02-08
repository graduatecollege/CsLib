using FluentValidation;

namespace Grad.CsLib.ClientErrorLogging;

/// <summary>
/// Validator for <see cref="ClientErrorLog"/> ensuring length limits and data constraints.
/// </summary>
public class ClientErrorLogValidator : AbstractValidator<ClientErrorLog>
{
    private const int MaxErrorTypeLength = 100;
    private const int MaxMessageLength = 1000;
    private const int MaxStackTraceLength = 5000;
    private const int MaxContextEntries = 20;
    private const int MaxContextKeyLength = 50;
    private const int MaxContextValueLength = 500;

    public ClientErrorLogValidator()
    {
        RuleFor(x => x.ErrorType)
            .NotEmpty()
            .WithMessage("ErrorType is required")
            .MaximumLength(MaxErrorTypeLength)
            .WithMessage($"ErrorType must not exceed {MaxErrorTypeLength} characters");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(MaxMessageLength)
            .WithMessage($"Message must not exceed {MaxMessageLength} characters");

        RuleFor(x => x.StackTrace)
            .MaximumLength(MaxStackTraceLength)
            .When(x => x.StackTrace != null)
            .WithMessage($"StackTrace must not exceed {MaxStackTraceLength} characters");

        RuleFor(x => x.Context)
            .Must(ctx => ctx == null || ctx.Count <= MaxContextEntries)
            .WithMessage($"Context must not exceed {MaxContextEntries} entries")
            .Must(ctx => ctx == null || ctx.Keys.All(k => k.Length <= MaxContextKeyLength))
            .WithMessage($"Context keys must not exceed {MaxContextKeyLength} characters")
            .Must(ctx => ctx == null || ctx.Values.All(v => IsValidContextValue(v)))
            .WithMessage($"Context values must be primitives (string, number, boolean, null) and strings must not exceed {MaxContextValueLength} characters");
    }

    private static bool IsValidContextValue(object? value)
    {
        if (value == null)
            return true;

        return value switch
        {
            string s => s.Length <= MaxContextValueLength,
            bool => true,
            byte or sbyte or short or ushort or int or uint or long or ulong => true,
            float or double or decimal => true,
            _ => false
        };
    }
}
