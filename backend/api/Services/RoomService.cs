using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;

namespace api.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly IErrorHandler _errorHandler;

    public RoomService(IRoomRepository roomRepository, IMapper mapper, IErrorHandler errorHandler)
    {
        _roomRepository = roomRepository;
        _mapper = mapper;
        _errorHandler = errorHandler;
        _errorHandler.InitService("ROOM", "ROOM_ERROR");
    }

    public Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(CreateRoomDTO roomDTO)
    {
        ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = true,
            Message = "",
        };
        try
        {
            Room roomMdl = _mapper.Map<Room>(roomDTO);
            _roomRepository.CreateRoomAsync(roomMdl);
        }
        catch (Exception ex) { }
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
