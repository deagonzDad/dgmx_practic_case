using System;
using api.Models;

namespace api.Repository.Interfaces;

public interface IRoomRepository
{
    Task CreateRoomAsync(Room room);
    Task<List<Room>> GetRoomsAsync(
        int limit,
        string? cursor,
        string? sortBy,
        string? sortOrder,
        string? filter
    );
    Task<Room> GetRoomByIdAsync(int Id);
    Task<Room> UpdateRoomAsync(Room room);
    Task LogicDeleteRoomAsync(Room room);
}
