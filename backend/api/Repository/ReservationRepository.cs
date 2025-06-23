using System;
using System.Linq.Expressions;
using api.Data;
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
        nameof(Reservation.TotalPrice),
        // nameof(Reservation.UserId),
        // nameof(Reservation.RoomId),
        // nameof(Reservation.PaymentId),
        // nameof(Reservation.User),
        // nameof(Reservation.Room),
        // nameof(Reservation.Payment),
    ];

    public async Task CreateReservation(Reservation reservation)
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

    public async Task<Reservation> GetReservationById(int reservationId)
    {
        Reservation reservation =
            await _context.Reservations.FirstOrDefaultAsync(el => el.Id == reservationId)
            ?? throw new ReservationNotFoundException(null);
        return reservation;
    }

    public async Task<(List<Reservation>, int?, int)> GetReservations(FilterParamsDTO filterParams)
    {
        // try
        // {
        IQueryable<Reservation> query = _context.Reservations.AsQueryable();
        if (!string.IsNullOrEmpty(filterParams.Filter))
        {
            string lowerCaseFilter = filterParams.Filter.ToLower();
            query = query.Where(data =>
                data.User.FirstName.Contains(lowerCaseFilter)
                || data.User.LastName.Contains(lowerCaseFilter)
                || data.Room.RoomNumber.ToString().Contains(lowerCaseFilter)
            );
        }
        bool hasSortBy =
            !string.IsNullOrEmpty(filterParams.SortBy)
            && _allowedSortByProperties.Contains(filterParams.SortBy);
        if (hasSortBy)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(Reservation), "r");
            MemberExpression property = Expression.Property(parameter, filterParams.SortBy!);
            Expression<Func<Reservation, object>> lambda = Expression.Lambda<
                Func<Reservation, object>
            >(Expression.Convert(property, typeof(object)), parameter);
            if (filterParams.SortOrder == 1)
            {
                query = query.OrderBy(lambda);
            }
            else
            {
                query = query.OrderByDescending(lambda);
            }
        }
        else
        {
            if (filterParams.SortOrder == 1)
            {
                query = query.OrderBy(r => r.Id);
            }
            else
            {
                query.OrderByDescending(r => r.Id);
            }
            filterParams.SortBy = _defaultSortBy;
        }
        if (
            hasSortBy
            && !filterParams.SortBy!.Equals(
                nameof(Reservation.Id),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            if (filterParams.SortOrder == 1)
            {
                query = ((IOrderedQueryable<Reservation>)query).ThenBy(r => r.Id);
            }
            else
            {
                query = ((IOrderedQueryable<Reservation>)query).ThenByDescending(r => r.Id);
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
        List<Reservation> reservations = await query.Take(filterParams.Limit + 1).ToListAsync();
        bool hasMore = reservations.Count > filterParams.Limit;
        if (hasMore)
        {
            reservations.RemoveAt(reservations.Count - 1);
        }
        int? nextLastId = hasMore ? reservations[^1].Id : null;
        return (reservations, nextLastId, totalCount);
    }

    // catch (Exception ex)
    // {
    //     throw;
    // }


    // public async Task<Reservation> UpdateReservation(Reservation reservation, int reservationId)
    // {
    //     try
    //     {
    //         Reservation reservationOld =
    //             await _context.Reservations.FindAsync(reservationId)
    //             ?? throw new ReservationNotFoundException(null);

    //     }
    //     catch (Exception ex)
    //     {
    //         throw new UpdateException(ex);
    //     }
    // }
}
