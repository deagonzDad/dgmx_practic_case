using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;

namespace api.Services.Interfaces;

public interface IReservationService
{
    Task<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>> CreateReservationAsync(
        CreateReservationDTO reservation
    );
    Task<DataListPaginationDTO<CreatedReservationDTO?, ErrorDTO?>> GetReservationsAsync(
        FilterParamsDTO filterParams
    );
    Task<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>> GetReservationByIdAsync(int reservationId);
}
