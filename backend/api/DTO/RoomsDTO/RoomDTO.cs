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
    [Range(1, int.MaxValue, ErrorMessage = "The value must be greater than 0")]
    public new int? Number { get; set; }
    public new RoomType Type { get; set; } = RoomType.Single;

    [Range(
        typeof(decimal),
        "0",
        "79228162514264337593543950335",
        ErrorMessage = "The value must be 0 or positive"
    )]
    public new decimal? PricePerNight { get; set; }

    [Required]
    public bool IsAvailable { get; set; }
}

public class CreatedRoomDTO : BaseRoomDTO
{
    public int Id { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<CreatedReservationDTO>? ReservationDTOs { get; set; }
}

public class RoomTypeDTO : IResponseData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
