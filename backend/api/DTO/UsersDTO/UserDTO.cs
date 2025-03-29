using System.ComponentModel.DataAnnotations;
using api.DTO.Interfaces;
using api.ValidationAttributes;

namespace api.DTO.UsersDTO;

public class BaseUserDTO : IResponseData
{
    [Required]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}

public class UserCreateDTO : BaseUserDTO
{
    [CustomPasswordValidation]
    public required string Password { get; set; }
    public List<int> Roles { get; set; } = [];
}

public class UserSignInDTO : BaseUserDTO
{
    [Required]
    public new required string Username { get; set; }
    public new string? Email { get; set; } = "";

    [CustomPasswordValidation]
    [Required]
    public required string Password { get; set; }
}

public class UserCreatedDTO : BaseUserDTO
{
    public int UserId { get; set; }

    public new string? Email { get; set; }

    // public List<int> Roles { get; set; } = [];
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
