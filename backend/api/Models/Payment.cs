using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ReservationId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    public Reservation? Reservation { get; set; }

    [Required]
    public virtual PaymentMethod PaymentMethod { get; set; }
}

public enum PaymentMethod
{
    CreditCard,
    Paypal,
    DebitCard,
}
