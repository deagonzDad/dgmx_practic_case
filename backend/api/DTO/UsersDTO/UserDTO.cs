using System.ComponentModel.DataAnnotations;
using api.DTO.Interfaces;
using api.ValidationAttributes;

namespace api.DTO.UsersDTO;

public class BaseUserDTO : IResponseData { }

public class UserCreateDTO : BaseUserDTO
{
    [Required]
    [UsernameExceptionValidation]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [CustomPasswordValidation]
    public required string Password { get; set; }
    public List<int> Roles { get; set; } = [];
}

public class UserSignInDTO : BaseUserDTO
{
    [UsernameExceptionValidation]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [CustomPasswordValidation]
    [Required]
    public required string Password { get; set; }

    [CustomValidation(typeof(UserSignInDTO), nameof(ValidateUsernameOrEmail))]
    public static ValidationResult? ValidateUsernameOrEmail(UserSignInDTO obj)
    {
        if (string.IsNullOrWhiteSpace(obj.Username) && string.IsNullOrWhiteSpace(obj.Email))
        {
            return new ValidationResult("Username or Email is required");
        }
        return ValidationResult.Success;
    }
}

public class UserCreatedDTO : BaseUserDTO
{
    public int UserId { get; set; }

    public required string Email { get; set; }
    public required string Username { get; set; }

    // public List<int> Roles { get; set; } = [];
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
