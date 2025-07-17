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

public class ReservationService(
    AppDbContext dbContext,
    IReservationRepository reservationRepository,
    IPaymentRepository paymentRepository,
    IRoomRepository roomRepository,
    IMapper mapper
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _dbContext = dbContext;

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
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
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
        catch (Exception)
        {
            throw;
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
            responseDTO.Data = dataList!;
            responseDTO.TotalRecords = totalCount;
            responseDTO.Next = nextCursor.ToString();
            responseDTO.Previous = filterParams.Cursor;
            return responseDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
