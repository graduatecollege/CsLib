using FluentValidation;

namespace Grad.CsLib.Validators;

/// <summary>
/// Provides custom validation rules for specific use cases in Graduate College applications.
/// </summary>
public static class GradValidators
{
    extension<T>(IRuleBuilder<T, string?> ruleBuilder)
    {
        /// <summary>
        /// Validates that a string is a valid program code.
        /// </summary>
        public IRuleBuilderOptions<T, string?> MustBeProgramCode()
        {
            return ruleBuilder
                .Matches(@"^1[0-9A-Z][A-Z]{2}\d{4}[A-Z]+$")
                .WithMessage("{PropertyName} must be a program code starting with 1.");
        }

        /// <summary>
        /// Validates that a string is a valid department code.
        /// </summary>
        public IRuleBuilderOptions<T, string?> MustBeDepartmentCode()
        {
            return ruleBuilder
                .Matches("^[0-9]{3,4}$")
                .WithMessage("{PropertyName} must be a numeric department code.");
        }

        /// <summary>
        /// Validates that a string is a valid term code, e.g. 120248.
        /// </summary>
        public IRuleBuilderOptions<T, string?> MustBeTermCode()
        {
            return ruleBuilder
                .Matches(@"^1\d{4}[158]$")
                .WithMessage("{PropertyName} must be in the format 1<year><term>, e.g. 120248.");
        }

        /// <summary>
        /// Validates that a string is a valid 9-digit numeric UIN (University Identification Number).
        /// </summary>
        public IRuleBuilderOptions<T, string?> MustBeUin()
        {
            return ruleBuilder
                .Matches(@"^6\d{8}$")
                .WithMessage("{PropertyName} must be a 9-digit number starting with 6.");
        }
        
        /// <summary>
        /// Validates that a string is a valid database identifier.
        /// </summary>
        public IRuleBuilderOptions<T, string> MustBeValidDbIdentifier()
        {
            return ruleBuilder
                .Matches("^[a-zA-Z_][a-zA-Z0-9_]*$")
                .WithMessage("{PropertyName} must be a valid database name containing only alphanumeric characters and underscores.");
        }
    }
}