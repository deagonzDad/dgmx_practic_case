using System;
using api.DTO.ResponseDTO;
using api.Models;

namespace api.Repository.Interfaces;

public interface IReservationRepository
{
    Task CreateReservation(Reservation reservation);

    // Task<Reservation> UpdateReservation(Reservation reservation, int reservationId);
    Task<Reservation> GetReservationById(int reservationId);
    Task<(List<Reservation>, int?, int)> GetReservations(FilterParamsDTO filterParams);
}
