using System;
using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Exceptions;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace api.Services;

public class RoomService(AppDbContext dbContext, IRoomRepository roomRepository, IMapper mapper)
    : IRoomService
{
    private readonly IRoomRepository _roomRepository = roomRepository;
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(
        CreateRoomDTO roomDTO
    )
    {
        ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            Room roomMdl = _mapper.Map<Room>(roomDTO);
            (bool isNew, Room room) = await _roomRepository.CreateRoomAsync(roomMdl);
            if (isNew)
            {
                room = _roomRepository.SetRoomActivation(room, true);
                await _roomRepository.UpdateRoomAsync(room, room.Id);
            }
            await transaction.CommitAsync();
            responseDTO.Data = _mapper.Map<CreatedRoomDTO>(roomMdl);
            responseDTO.Message = "Success in the room creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (Exception)
        {
            throw;
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
        catch (Exception)
        {
            throw;
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
        using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            Room roomObj = _mapper.Map<Room>(room);
            Room roomMdl = await _roomRepository.UpdateRoomAsync(roomObj, IdRoom);
            responseDTO.Data = _mapper.Map<CreatedRoomDTO>(roomMdl);
            responseDTO.Message = "Success in the room update";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ResponseDTO<BaseRoomDTO?, ErrorDTO?>> DeleteRoomAsync(int roomId)
    {
        ResponseDTO<BaseRoomDTO?, ErrorDTO?> responseDTO = new() { Success = false, Message = "" };
        using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            Room roomToDel = await _roomRepository.GetRoomByIdAsync(roomId);
            await _roomRepository.LogicDeleteRoomAsync(roomToDel);
            await transaction.CommitAsync();
            responseDTO.Message = "Success in the room deletion";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> GetRoomByIdAsync(int roomId)
    {
        ResponseDTO<CreatedRoomDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            Room room = await _roomRepository.GetRoomByIdAsync(roomId);
            responseDTO.Data = _mapper.Map<CreatedRoomDTO>(room);
            responseDTO.Message = "Success in the room retrieval";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
