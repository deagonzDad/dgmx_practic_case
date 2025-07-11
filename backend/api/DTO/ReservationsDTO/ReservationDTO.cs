using api.DTO.Interfaces;
using api.DTO.PaymentDTO;
using api.DTO.RoomsDTO;
using api.DTO.UsersDTO;
using api.Models;

namespace api.DTO.ReservationsDTO;

public class BaseReservationDTO : IResponseData { }

public class CreateReservationDTO : BaseReservationDTO
{
    public DateTime InDate { get; set; }
    public int Guest { get; set; }
    public int ClientId { get; set; }
    public int RoomId { get; set; }

    // public decimal Price { get; set; }
    public PaymentMethod Method { get; set; }
}

public class BaseCreatedReservationDTO : BaseReservationDTO
{
    public int ReservationId { get; set; }
    public DateTime InDate { get; set; }
    public DateTime? OutDate { get; set; }
    public int Guests { get; set; }
}

public class CreatedReservationListDTO : BaseCreatedReservationDTO
{
    public string? Username { get; set; }
    public int? RoomNumber { get; set; }
}

public class CreatedReservationDTO : BaseCreatedReservationDTO
{
    public UserCreatedDTO User { get; set; } = null!;
    public CreatedRoomDTO Room { get; set; } = null!;
    public CreatedPaymentDTO Payment { get; set; } = null!;
}
