using System.ComponentModel.DataAnnotations;
using api.Helpers.Instances;

namespace api.ValidationAttributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class UsernameExceptionValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext valContext)
    {
        IRegexController _regexController =
            valContext.GetService<IRegexController>()
            ?? throw new Exception("Service not implemented");
        if (value == null)
        {
            return new ValidationResult(ErrorMessage = "Username cannot be null");
        }
        string? username = value.ToString();
        if (username == null)
        {
            return new ValidationResult("The username field is empty");
        }
        if (_regexController.FilterExcludeSymbols().IsMatch(username))
        {
            return new ValidationResult("The username can't have @ symbols");
        }
        return ValidationResult.Success;
    }
}
