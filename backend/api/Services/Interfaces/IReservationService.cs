using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;

namespace api.Services.Interfaces;

public interface IReservationService
{
    Task<ResponseDTO<CreatedReservationListDTO?, ErrorDTO?>> CreateReservationAsync(
        CreateReservationDTO reservation
    );
    Task<DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?>> GetReservationsAsync(
        FilterParamsDTO filterParams
    );
    Task<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>> GetReservationByIdAsync(int reservationId);
}
