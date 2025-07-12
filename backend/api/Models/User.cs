using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Password { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = "";

    [MaxLength(100)]
    public string LastName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<Reservation> Reservations { get; set; } = [];

    // public virtual ICollection<UserReservation> UserReservations { get; set; } = [];

    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
