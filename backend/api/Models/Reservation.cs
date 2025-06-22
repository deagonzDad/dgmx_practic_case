using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Reservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public int NumberOfGuests { get; set; }

    [Required]
    public decimal TotalPrice { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int PaymentId { get; set; }
    public User User { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}
