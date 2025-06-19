using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Exceptions;
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
            (bool isNew, Room room) = await _roomRepository.CreateRoomAsync(roomMdl);
            if (isNew)
            {
                room = _roomRepository.SetRoomActivation(room, true);
                await _roomRepository.UpdateRoomAsync(room, room.Id);
            }
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

    public async Task<DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>> GetRoomsAsync(
        FilterParamsDTO filterParams
    )
    {
        DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new() { };
        try
        {
            (List<Room> query, int? nextCursor, int totalCount) =
                await _roomRepository.GetRoomsAsync(filterParams);
            List<CreatedRoomDTO> roomMap = _mapper.Map<List<CreatedRoomDTO>>(query);
            responseDTO.Data = roomMap!;
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

    public async Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> UpdateRoomAsync(
        UpdateRoomDTO room,
        int IdRoom
    )
    {
        ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            Room roomObj = _mapper.Map<Room>(room);
            Room roomMdl = await _roomRepository.UpdateRoomAsync(roomObj, IdRoom);
            responseDTO.Data = _mapper.Map<CreatedRoomDTO>(roomMdl);
            responseDTO.Message = "Success in the room update";
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

    public Task<ResponseDTO<BaseRoomDTO?, ErrorDTO?>> DeleteRoomAsync(int Id)
    {
        try { }
        catch (Exception ex) { }
        throw new NotImplementedException();
    }
}
