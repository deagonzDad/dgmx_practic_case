using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public decimal AmountPerNight { get; set; }
    public decimal? TotalAmount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
}

public enum PaymentMethod
{
    CreditCard,
    Paypal,
    DebitCard,
}

public enum PaymentStatus
{
    Pending,
    Successful,
    Failed,
    Refunded,
    Cancelled,
}
