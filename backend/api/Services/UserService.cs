using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;

namespace api.Services;

public class UserService(IMapper mapper, IUserRepository userRepository) : IUserService
{
    private readonly IMapper _mapper = mapper;

    private readonly IUserRepository _userRepository = userRepository;

    public async Task<DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>> GetUsersAsync(
        FilterParamsDTO filterParams
    )
    {
        DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?> responseDTO = new() { };
        try
        {
            (List<User> query, int? nextCursor, int totalCount) =
                await _userRepository.GetUsersAsync(filterParams);
            List<UserCreatedDTO> userMap = _mapper.Map<List<UserCreatedDTO>>(query);
            responseDTO.Data = userMap!;
            responseDTO.Next = nextCursor.ToString();
            responseDTO.Previous = filterParams.Cursor;
            responseDTO.TotalRecords = totalCount;
            return responseDTO;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
