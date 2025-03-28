using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using api.Services.Interfaces;

namespace api.Services;

public class RoomService : IRoomService
{
    public Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(CreateRoomDTO roomDTO)
    {
        throw new NotImplementedException();
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
