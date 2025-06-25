using api.DTO.Interfaces;
using api.Models;

namespace api.DTO.ReservationsDTO;

public class BaseReservationDTO : IResponseData { }

public class CreateReservationDTO : BaseReservationDTO
{
    public DateTime InDate { get; set; }
    public int Guest { get; set; }
    public int ClientId { get; set; }
    public int RoomId { get; set; }
    public decimal Price { get; set; }
    public PaymentMethod Method { get; set; }
}

public class CreatedReservationDTO : BaseReservationDTO { }
