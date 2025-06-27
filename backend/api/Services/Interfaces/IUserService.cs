using System;
using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;

namespace api.Services.Interfaces;

public interface IUserService
{
    Task<DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>> GetUsersAsync(
        FilterParamsDTO filterParams
    );
}
