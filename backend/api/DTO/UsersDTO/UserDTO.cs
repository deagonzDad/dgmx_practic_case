using System.ComponentModel.DataAnnotations;
using api.DTO.Interfaces;
using api.ValidationAttributes;

namespace api.DTO.UsersDTO;

public class UserCreateDTO
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [CustomPasswordValidation]
    public required string Password { get; set; }
    public List<int> Roles { get; set; } = [];
}

public class UserSignInDTO
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class UserCreatedDTO : IResponseData
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }

    // public List<int> Roles { get; set; } = [];
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
