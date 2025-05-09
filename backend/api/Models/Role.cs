using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models;

[Index(nameof(Name), IsUnique = true)]
public class Role : IEquatable<Role>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }
    public virtual ICollection<User> Users { get; set; } = [];
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public bool Equals(Role? other)
    {
        if (other == null)
        {
            return false;
        }
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Role);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
