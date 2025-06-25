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
        IQueryable<Room> query = _context.Rooms.AsQueryable().AsNoTracking();
        query = query.Where(data => data.IsActive == filterParams.IsActive);
        if (!string.IsNullOrEmpty(filterParams.Filter))
        {
            string lowerCaseFilter = filterParams.Filter.ToLower();
            query = query.Where(data =>
                data.RoomNumber.ToString().Contains(lowerCaseFilter.ToString())
            );
        }
        bool hasSortBy =
            !string.IsNullOrWhiteSpace(filterParams.SortBy)
            && _allowedSortByProperties.Contains(filterParams.SortBy);
        if (hasSortBy)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(Room), "r");
            MemberExpression property = Expression.Property(parameter, filterParams.SortBy!);
            Expression<Func<Room, object>> lambda = Expression.Lambda<Func<Room, object>>(
                Expression.Convert(property, typeof(object)),
                parameter
            );
            if (filterParams.SortOrder == 1)
            {
                query = query.OrderBy(lambda);
            }
            {
                query = query.OrderByDescending(lambda);
            }
        }
        else
        {
            if (filterParams.SortOrder == 1)
            {
                query = query.OrderBy(r => r.RoomNumber);
            }
            else
            {
                query = query.OrderByDescending(r => r.RoomNumber);
            }
            filterParams.SortBy = _defaultSortBy;
        }
        if (
            hasSortBy
            && !filterParams.SortBy!.Equals(nameof(Room.Id), StringComparison.OrdinalIgnoreCase)
        )
        {
            if (filterParams.SortOrder == 1)
            {
                query = ((IOrderedQueryable<Room>)query).ThenBy(r => r.Id);
            }
            else
            {
                query = ((IOrderedQueryable<Room>)query).ThenByDescending(r => r.Id);
            }
        }
        int totalCount = await query.CountAsync();
        if (
            !string.IsNullOrWhiteSpace(filterParams.Cursor)
            && int.TryParse(filterParams.Cursor, out int cursorId)
        )
        {
            if (filterParams.SortOrder == 1)
            {
                query = query.Where(r => r.Id > cursorId);
            }
            else
            {
                query = query.Where(r => r.Id < cursorId);
            }
        }
        List<Room> rooms = await query.Take(filterParams.Limit + 1).ToListAsync();
        bool hasMore = rooms.Count > filterParams.Limit;
        if (hasMore)
        {
            rooms.RemoveAt(rooms.Count - 1);
        }
        int? nextLastId = hasMore ? rooms[^1].Id : null;
        return (rooms, nextLastId, totalCount);
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
