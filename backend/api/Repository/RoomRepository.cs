using System;
using api.Data;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    private readonly AppDbContext _context = context;

    public async Task CreateRoomAsync(Room room)
    {
        Room? existedRoom = await _context.Rooms.FirstOrDefaultAsync(el =>
            el.RoomNumber == room.RoomNumber
        );
        if (existedRoom != null)
        {
            SetRoomActivation(existedRoom, false);
        }
        else
        {
            await CreateNewRoomAsync(room);
        }
        await _context.SaveChangesAsync();
    }

    private static void SetRoomActivation(Room existedRoom, bool deactivate)
    {
        existedRoom.IsActive = !deactivate;
    }

    private async Task CreateNewRoomAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
    }

    public async Task LogicDeleteRoomAsync(Room room)
    {
        SetRoomActivation(room, true);
        await _context.SaveChangesAsync();
    }

    public async Task<Room> GetRoomByIdAsync(int Id)
    {
        Room room =
            await _context.Rooms.FirstOrDefaultAsync(el => el.Id == Id)
            ?? throw new RoomNotFoundException();
        return room;
    }

    public Task<List<Room>> GetRoomsAsync(
        int limit,
        string? cursor,
        string? sortBy,
        string? sortOrder,
        string? filter
    )
    {
        throw new NotImplementedException();
    }

    public async Task<Room> UpdateRoomAsync(Room updatedRoom)
    {
        Room existedRoom =
            await _context.Rooms.FindAsync(updatedRoom.Id) ?? throw new RoomNotFoundException();
        _context.Entry(existedRoom).CurrentValues.SetValues(updatedRoom);
        await _context.SaveChangesAsync();
        return existedRoom;
    }
}
