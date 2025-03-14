using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    [Required]
    [MaxLength(100)]
    public required string Username {get; set;}
    [Required]
    [EmailAddress]
    public required string Email{get; set;}
    [Required]
    [MaxLength(255)]
    public required string Password {get; set;}
    [MaxLength(100)]
    public string FirstName {get; set;} = "";
    [MaxLength(100)]
    public string LastName {get; set;} = "";
    public ICollection<Reservation> Reservations {get;set;} = [];
    public ICollection<Role> Roles {get; set;} = [];
    
}
public class Role {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get; set;}
    [Required]
    public required string Name {get; set;}
    public ICollection<User> Users {get; set;} = [];
}
public class Room {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get; set;}
        [Required]
        public int RoomNumber { get; set; }
        [Required]
        public RoomType RoomType { get; set; }
        [Required]
        public decimal PricePerNight { get; set; }
        public ICollection<Reservation> Reservations { get; set; } = [];
}
public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int RoomId { get; set; }
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }
        [Required]
        public int NumberOfGuests { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        public User? User { get; set; }
        public Room? Room { get; set; }
        public Payment? Payment {get; set;}
    }

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
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public Reservation? Reservation { get; set; }
    }

public enum RoomType{
    Single,
    Double,
    Deluxe,
    Family
}

public enum PaymentMethod{
    CreditCard,
    Paypal,
    DebitCard,
}