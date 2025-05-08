using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly IErrorHandler _errorHandler;
    private readonly ILogger<RoomService> _logger;

    public RoomService(
        IRoomRepository roomRepository,
        IMapper mapper,
        IErrorHandler errorHandler,
        ILogger<RoomService> logger
    )
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
        _errorHandler = errorHandler;
        _logger = logger;
        _errorHandler.InitService("ROOM", "ROOM_ERROR");
    }

    public async Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(
        CreateRoomDTO roomDTO
    )
    {
        ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            Room roomMdl = _mapper.Map<Room>(roomDTO);
            await _roomRepository.CreateRoomAsync(roomMdl);
            responseDTO.Data = _mapper.Map<CreatedRoomDTO>(roomMdl);
            responseDTO.Message = "Success in the room creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (DbUpdateException ex)
        {
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in room creation",
                400,
                _logger
            );
        }
    }

    public Task<ResponseDTO<DataListPaginationDTO<CreatedRoomDTO?>?, ErrorDTO?>> GetRoomsAsync(
        int limit,
        string? cursor,
        string? sortBy,
        string? sortOrder,
        string? filter
    )
    {
        throw new NotImplementedException();
    }
}
