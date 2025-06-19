using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
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

    public async Task<(bool, Room)> CreateRoomAsync(Room room)
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

    public async Task LogicDeleteRoomAsync(Room room)
    {
        SetRoomActivation(room, false);
        await _context.SaveChangesAsync();
    }

    public async Task<Room> GetRoomByIdAsync(int Id)
    {
        Room room =
            await _context.Rooms.FirstOrDefaultAsync(el => el.Id == Id)
            ?? throw new RoomNotFoundException();
        return room;
    }

    public async Task<(List<Room>, int?, int)> GetRoomsAsync(FilterParamsDTO filterParams)
    {
        try
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
            int totalCount = await query.CountAsync();
            List<Room> rooms = await query.Take(filterParams.Limit + 1).ToListAsync();
            bool hasMore = rooms.Count > filterParams.Limit;

            int? nextLastId = hasMore ? rooms[^1].Id : null;

            if (hasMore)
            {
                rooms.RemoveAt(rooms.Count - 1);
            }
            return (rooms, nextLastId, totalCount);
        }
        catch (Exception)
        {
            throw new RoomNotFoundException();
        }
    }

    public async Task<Room> UpdateRoomAsync(Room updatedRoom, int roomId)
    {
        try
        {
            Room existedRoom =
                await _context.Rooms.FindAsync(updatedRoom.Id) ?? throw new RoomNotFoundException();
            _context.Entry(existedRoom).CurrentValues.SetValues(updatedRoom);
            await _context.SaveChangesAsync();
            return existedRoom;
        }
        catch (Exception)
        {
            throw new RoomNotFoundException();
        }
    }

    public Room SetRoomActivation(Room existedRoom, bool active)
    {
        existedRoom.IsActive = active;
        return existedRoom;
    }
}
