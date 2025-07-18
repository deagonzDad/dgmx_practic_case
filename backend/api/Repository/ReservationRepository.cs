using System;
using System.Linq.Expressions;
using api.Data;
using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Models;
using api.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Repository;

public class ReservationRepository(AppDbContext context) : IReservationRepository
{
    private readonly AppDbContext _context = context;
    private readonly string _defaultSortBy = nameof(Reservation.Id);
    private readonly HashSet<string> _allowedSortByProperties =
    [
        nameof(Reservation.Id),
        nameof(Reservation.NumberOfGuests),
    ];

    public async Task CreateReservationAsync(Reservation reservation)
    {
        try
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new UpdateException(ex);
        }
    }

    public async Task<Reservation> GetReservationByIdAsync(int reservationId)
    {
        Reservation reservation =
            await _context
                .Reservations.Include(r => r.User)
                .Include(r => r.Room)
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(el => el.Id == reservationId)
            ?? throw new ReservationNotFoundException(null);
        return reservation;
    }

    public async Task<(List<CreatedReservationListDTO>, int?, int)> GetReservations(
        FilterParamsDTO filterParams
    )
    {
        IQueryable<Reservation> query = _context
            .Reservations.Include(r => r.User)
            .Include(r => r.Room)
            .AsNoTracking();

        // Filtering
        if (!string.IsNullOrEmpty(filterParams.Filter))
        {
            string lowerCaseFilter = filterParams.Filter.ToLower();
            if (int.TryParse(lowerCaseFilter, out int roomNumber))
            {
                query = query.Where(r => r.Room.RoomNumber == roomNumber);
            }
            else
            {
                query = query.Where(data =>
                    data.User.FirstName.ToLower().Contains(lowerCaseFilter)
                    || data.User.LastName.ToLower().Contains(lowerCaseFilter)
                );
            }
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
            nameof(Reservation.NumberOfGuests) => isDescending
                ? query.OrderByDescending(r => r.NumberOfGuests)
                : query.OrderBy(r => r.NumberOfGuests),
            _ => isDescending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
        };

        query = ((IOrderedQueryable<Reservation>)query).ThenBy(r => r.Id);

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

        var reservations = await query
            .Select(r => new CreatedReservationListDTO
            {
                ReservationId = r.Id,
                Username = r.User.Username,
                RoomNumber = r.Room.RoomNumber,
                InDate = r.CheckInDate,
                Guests = r.NumberOfGuests,
                OutDate = r.CheckOutDate,
            })
            .Take(filterParams.Limit + 1)
            .ToListAsync();

        bool hasMore = reservations.Count > filterParams.Limit;
        if (hasMore)
        {
            reservations.RemoveAt(reservations.Count - 1);
        }

        int? nextCursor = hasMore ? reservations.LastOrDefault()?.ReservationId : null;

        return (reservations, nextCursor, totalCount);
    }
}
