using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;

namespace api.Services.Interfaces;

public interface IRoomService
{
    Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(CreateRoomDTO roomDTO);
    Task<ResponseDTO<DataListPaginationDTO<CreatedRoomDTO?>?, ErrorDTO?>> GetRoomsAsync(
        int limit,
        string? cursor,
        string? sortBy,
        string? sortOrder,
        string? filter
    );
}
