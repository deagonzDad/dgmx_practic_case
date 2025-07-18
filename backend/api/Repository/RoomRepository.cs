using System.Linq.Expressions;
using api.Data;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    private readonly string _defaultSortBy = nameof(Room.RoomNumber);
    private readonly AppDbContext _context = context;
    private readonly HashSet<string> _allowedSortByProperties =
    [
        nameof(Room.RoomNumber),
        nameof(Room.Id),
    ];

    public async Task<decimal> GetPriceByIdAsync(int Id)
    {
        try
        {
            decimal? price = await _context
                .Rooms.Where(ro => ro.Id == Id)
                .Select(r => (decimal?)r.PricePerNight)
                .FirstOrDefaultAsync();
            if (!price.HasValue)
            {
                throw new RoomNotFoundException(null);
            }
            return (decimal)price;
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
    }

    public async Task<(bool, Room)> CreateRoomAsync(Room room)
    {
        try
        {
            Room? existedRoom = await _context.Rooms.FirstOrDefaultAsync(el =>
                el.RoomNumber == room.RoomNumber
            );
            bool isNew = existedRoom == null;
            if (isNew)
            {
                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();
                return (isNew, room);
            }
            return (isNew, existedRoom!);
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
    }

    public async Task LogicDeleteRoomAsync(Room room)
    {
        SetRoomActivation(room, false);
        await _context.SaveChangesAsync();
    }

    public async Task<Room> GetRoomByIdAsync(int Id)
    {
        Room room =
            await _context.Rooms.FirstOrDefaultAsync(el => el.Id == Id)
            ?? throw new RoomNotFoundException(null);
        return room;
    }

    public async Task<(List<Room>, int?, int)> GetRoomsAsync(FilterParamsDTO filterParams)
    {
        IQueryable<Room> query = _context.Rooms.AsNoTracking();

        // Filtering
        query = query.Where(data => data.IsActive == filterParams.IsActive);
        if (
            !string.IsNullOrEmpty(filterParams.Filter)
            && int.TryParse(filterParams.Filter, out int roomNumber)
        )
        {
            query = query.Where(data => data.RoomNumber == roomNumber);
        }

        // Sorting
        var sortBy =
            (
                string.IsNullOrWhiteSpace(filterParams.SortBy)
                || !_allowedSortByProperties.Contains(filterParams.SortBy)
            )
                ? _defaultSortBy
                : filterParams.SortBy;

        var isDescending = filterParams.SortOrder == 0;

        query = sortBy switch
        {
            nameof(Room.RoomNumber) => isDescending
                ? query.OrderByDescending(r => r.RoomNumber)
                : query.OrderBy(r => r.RoomNumber),
            nameof(Room.Id) => isDescending
                ? query.OrderByDescending(r => r.Id)
                : query.OrderBy(r => r.Id),
            _ => isDescending
                ? query.OrderByDescending(r => r.RoomNumber)
                : query.OrderBy(r => r.RoomNumber),
        };

        // Secondary sort for consistent ordering
        query = ((IOrderedQueryable<Room>)query).ThenBy(r => r.Id);

        int totalCount = await query.CountAsync();

        // Pagination
        if (
            !string.IsNullOrWhiteSpace(filterParams.Cursor)
            && int.TryParse(filterParams.Cursor, out int cursorId)
        )
        {
            query = isDescending
                ? query.Where(r => r.Id < cursorId)
                : query.Where(r => r.Id > cursorId);
        }

        var rooms = await query.Take(filterParams.Limit + 1).ToListAsync();

        bool hasMore = rooms.Count > filterParams.Limit;
        if (hasMore)
        {
            rooms.RemoveAt(rooms.Count - 1);
        }

        int? nextCursor = hasMore ? rooms.LastOrDefault()?.Id : null;

        return (rooms, nextCursor, totalCount);
    }

    public async Task<Room> UpdateRoomAsync(Room updatedRoom, int roomId)
    {
        try
        {
            Room existedRoom =
                await _context.Rooms.FindAsync(roomId) ?? throw new RoomNotFoundException(null);
            existedRoom.RoomNumber = updatedRoom.RoomNumber;
            existedRoom.RoomType = updatedRoom.RoomType;
            existedRoom.PricePerNight = updatedRoom.PricePerNight;
            existedRoom.IsAvailable = updatedRoom.IsAvailable;
            await _context.SaveChangesAsync();
            return existedRoom;
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
    }

    public Room SetRoomActivation(Room existedRoom, bool active)
    {
        existedRoom.IsActive = active;
        return existedRoom;
    }
}
