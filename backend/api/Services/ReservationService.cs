using System;
using api.Data;
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
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly AppDbContext _dbContext;

    public ReservationService(
        AppDbContext dbContext,
        IReservationRepository reservationRepository,
        IPaymentRepository paymentRepository,
        IRoomRepository roomRepository,
        IMapper mapper,
        IErrorHandler errorHandler,
        ILogger<ReservationService> logger
    )
    {
        _dbContext = dbContext;
        _reservationRepository = reservationRepository;
        _paymentRepository = paymentRepository;
        _roomRepository = roomRepository;
        _mapper = mapper;
        _errorHandler = errorHandler;
        _logger = logger;
        errorHandler.InitService("RESERVATION", "RESERVATION_ERROR");
    }

    public async Task<ResponseDTO<CreatedReservationListDTO?, ErrorDTO?>> CreateReservationAsync(
        CreateReservationDTO reservation
    )
    {
        ResponseDTO<CreatedReservationListDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            decimal price = await _roomRepository.GetPriceByIdAsync(reservation.RoomId);
            Payment payment = _mapper.Map<Payment>(reservation);
            payment.AmountPerNight = price;
            (bool isSaveIt, Payment paymentCreated) = await _paymentRepository.CreatePaymentAsync(
                payment
            );
            Reservation reservationMdl = _mapper.Map<Reservation>(reservation);
            reservationMdl.Payment = paymentCreated;
            await _reservationRepository.CreateReservationAsync(reservationMdl);
            await transaction.CommitAsync();
            responseDTO.Data = _mapper.Map<CreatedReservationListDTO>(reservationMdl);
            responseDTO.Message = "Success in the reservation creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (UpdateException ex)
        {
            await transaction.RollbackAsync();
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in room creation",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
        finally
        {
            await transaction.DisposeAsync();
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
                "Reservation Not Found",
                "Reservation not found in the database",
                StatusCodes.Status404NotFound,
                _logger
            );
        }
    }

    public async Task<
        DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?>
    > GetReservationsAsync(FilterParamsDTO filterParams)
    {
        DataListPaginationDTO<CreatedReservationListDTO?, ErrorDTO?> responseDTO = new() { };
        try
        {
            (List<CreatedReservationListDTO> dataList, int? nextCursor, int totalCount) =
                await _reservationRepository.GetReservations(filterParams);
            // List<CreatedReservationListDTO> reservationsMap = _mapper.Map<
            //     List<CreatedReservationListDTO>
            // >(query);
            responseDTO.Data = dataList!;
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
