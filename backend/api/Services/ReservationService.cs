using System;
using api.DTO.ReservationsDTO;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;

namespace api.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;
    private readonly IErrorHandler _errorHandler;

    public ReservationService(
        IReservationRepository reservationRepository,
        IMapper mapper,
        IErrorHandler errorHandler,
        ILogger<ReservationService> logger
    )
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
        _errorHandler = errorHandler;
        _logger = logger;
        errorHandler.InitService("RESERVATION", "RESERVATION_ERROR");
    }

    public async Task<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>> CreateReservationAsync(
        CreateReservationDTO reservation
    )
    {
        ResponseDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            Reservation reservationMdl = _mapper.Map<Reservation>(reservation);
            await _reservationRepository.CreateReservationAsync(reservationMdl);
            responseDTO.Data = _mapper.Map<CreatedReservationDTO>(reservationMdl);
            responseDTO.Message = "Success in the reservation creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (UpdateException ex)
        {
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in room creation",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
    }

    public async Task<ResponseDTO<CreatedReservationDTO?, ErrorDTO?>> GetReservationByIdAsync(
        int reservationId
    )
    {
        ResponseDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            Reservation reservation = await _reservationRepository.GetReservationByIdAsync(
                reservationId
            );
            responseDTO.Data = _mapper.Map<CreatedReservationDTO>(reservation);
            responseDTO.Message = "Success in the reservation creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (ReservationNotFoundException ex)
        {
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "Room Not Found",
                "Room not found in the database",
                StatusCodes.Status404NotFound,
                _logger
            );
        }
    }

    public async Task<
        DataListPaginationDTO<CreatedReservationDTO?, ErrorDTO?>
    > GetReservationsAsync(FilterParamsDTO filterParams)
    {
        DataListPaginationDTO<CreatedReservationDTO?, ErrorDTO?> responseDTO = new() { };
        try
        {
            (List<Reservation> query, int? nextCursor, int totalCount) =
                await _reservationRepository.GetReservations(filterParams);
            List<CreatedReservationDTO> reservationsMap = _mapper.Map<List<CreatedReservationDTO>>(
                query
            );
            responseDTO.Data = reservationsMap!;
            responseDTO.TotalRecords = totalCount;
            responseDTO.Next = nextCursor.ToString();
            responseDTO.Previous = filterParams.Cursor;
            return responseDTO;
        }
        catch (RoomNotFoundException ex)
        {
            return _errorHandler.CreateErrorListRes(
                ex,
                responseDTO,
                "Room not found.",
                "Room not found in the database",
                StatusCodes.Status404NotFound,
                _logger
            );
        }
        catch (UnauthorizedActionException ex)
        {
            return _errorHandler.CreateErrorListRes(
                ex,
                responseDTO,
                "Room not found",
                "Room not found in the database",
                StatusCodes.Status403Forbidden,
                _logger
            );
        }
    }
}
