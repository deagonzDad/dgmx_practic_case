using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using api.DTO.Interfaces;
using api.DTO.ReservationsDTO;
using api.Models;

namespace api.DTO.RoomsDTO;

public class BaseRoomDTO : IResponseData
{
    [Required]
    public int Number { get; set; }

    [Required]
    public RoomType Type { get; set; }

    [Required]
    public decimal PricePerNight { get; set; }
}

public class CreateRoomDTO : BaseRoomDTO { }

public class UpdateRoomDTO : BaseRoomDTO
{
    [Required]
    public bool IsAvailable { get; set; }

    [Required]
    public bool IsActive { get; set; }

    // public int RoomId { get; set; }
}

public class CreatedRoomDTO : CreateRoomDTO
{
    public int Id { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<CreatedReservationDTO>? ReservationDTOs { get; set; }
}
