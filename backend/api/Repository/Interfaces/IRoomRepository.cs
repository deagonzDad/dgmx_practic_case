using api.DTO.ResponseDTO;
using api.Models;

namespace api.Repository.Interfaces;

public interface IRoomRepository
{
    Task<(bool, Room)> CreateRoomAsync(Room room);
    Task<(List<Room>, int?, int)> GetRoomsAsync(FilterParamsDTO filterParams);
    Task<Room> GetRoomByIdAsync(int Id);
    Task<decimal> GetPriceByIdAsync(int Id);
    Task<Room> UpdateRoomAsync(Room room, int roomId);
    Task LogicDeleteRoomAsync(Room room);
    Room SetRoomActivation(Room existedRoom, bool deactivate);
}
