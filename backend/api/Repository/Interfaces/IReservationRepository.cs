using System;
using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
using api.Models;

namespace api.Repository.Interfaces;

public interface IReservationRepository
{
    Task CreateReservationAsync(Reservation reservation);

    // Task<Reservation> UpdateReservation(Reservation reservation, int reservationId);
    Task<Reservation> GetReservationByIdAsync(int reservationId);
    Task<(List<CreatedReservationListDTO>, int?, int)> GetReservations(
        FilterParamsDTO filterParams
    );
}
