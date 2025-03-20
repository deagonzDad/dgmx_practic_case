using System;
using api.DTO.ResponseDTO;
using api.DTO.SetttingsDTO;
using api.DTO.UsersDTO;
using api.Models;

namespace api.Services.Interfaces;

public interface IAuthService
{
    Task<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>> LoginAsync(UserSignInDTO userDTO);
    Task<User> SignupAsync(UserCreateDTO userDTO);
}
