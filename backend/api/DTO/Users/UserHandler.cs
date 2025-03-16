using System.ComponentModel.DataAnnotations;

namespace api.DTO.Users;

public class UserCreateDTO
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
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
