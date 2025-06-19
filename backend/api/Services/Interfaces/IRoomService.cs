using System;
using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;
using Microsoft.AspNetCore.Http.HttpResults;

namespace api.Services.Interfaces;

public interface IRoomService
{
    Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> CreateRoomAsync(CreateRoomDTO roomDTO);
    Task<DataListPaginationDTO<CreatedRoomDTO?, ErrorDTO?>> GetRoomsAsync(
        FilterParamsDTO filterParams
    );
    Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> UpdateRoomAsync(
        UpdateRoomDTO roomDTO,
        int IdRoom
    );
}
