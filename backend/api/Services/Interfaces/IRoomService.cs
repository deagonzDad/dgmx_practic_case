using api.DTO.ResponseDTO;
using api.DTO.RoomsDTO;

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
    Task<ResponseDTO<BaseRoomDTO?, ErrorDTO?>> DeleteRoomAsync(int roomId);
    Task<ResponseDTO<CreatedRoomDTO?, ErrorDTO?>> GetRoomByIdAsync(int roomId);
}
