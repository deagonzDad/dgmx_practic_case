using System.ComponentModel.DataAnnotations;
using api.ValidationAttributes.Interfaces;

namespace api.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class CustomPasswordValidation : ValidationAttribute
{
    private const int MinimumLenght = 8;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        IRegexController _regexController =
            validationContext.GetService<IRegexController>()
            ?? throw new Exception("Service not implemented");
        if (value == null)
        {
            return new ValidationResult(ErrorMessage = "Password is required");
        }
        string password = value.ToString() ?? "";
        if (password.Length < MinimumLenght)
        {
            return new ValidationResult(
                ErrorMessage = "Password must be at least 8 characters long"
            );
        }
        if (!_regexController.UpperCaseRegex().IsMatch(password))
        {
            return new ValidationResult(
                ErrorMessage = "Password must contain at least one uppercase letter"
            );
        }
        if (!_regexController.SpecialCharacterRegex().IsMatch(password))
        {
            return new ValidationResult(
                ErrorMessage = "Password must contain at least one special character."
            );
        }
        return ValidationResult.Success;
    }
}
