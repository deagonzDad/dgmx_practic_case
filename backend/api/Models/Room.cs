using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Room
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int RoomNumber { get; set; }

    [Required]
    public RoomType RoomType { get; set; }

    [Required]
    public decimal PricePerNight { get; set; }

    [Required]
    public bool IsAvailable { get; set; } = true;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public virtual ICollection<Reservation> Reservations { get; set; } = [];
}

public enum RoomType
{
    Single,
    Double,
    Deluxe,
    Family,
}
