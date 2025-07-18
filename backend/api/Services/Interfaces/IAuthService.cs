using api.DTO.ResponseDTO;
using api.DTO.SettingsDTO;
using api.DTO.UsersDTO;

namespace api.Services.Interfaces;

public interface IAuthService
{
    Task<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>> LoginAsync(UserSignInDTO userDTO);
    Task<ResponseDTO<UserCreatedDTO?, ErrorDTO?>> SignupAsync(UserCreateDTO userDTO);
}
