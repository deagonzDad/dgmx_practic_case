using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace api.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed partial class CustomPasswordValidation : ValidationAttribute
{
    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCaseRegex();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex SpecialCharacterRegex();

    private const int MinimumLenght = 8;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("Password is required");
        }
        string password = value?.ToString() ?? "";
        if (password.Length < MinimumLenght)
        {
            return new ValidationResult("Password must be at least 8 characters long");
        }
        if (!UpperCaseRegex().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one uppercase letter");
        }
        if (!SpecialCharacterRegex().IsMatch(password))
        {
            return new ValidationResult("Password must contain at least one special character.");
        }
        return ValidationResult.Success;
    }
}
