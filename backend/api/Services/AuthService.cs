using System;
using api.DTO.ResponseDTO;
using api.DTO.SetttingsDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;

namespace api.Services;

public class AuthService(
    IUserRepository userRepository,
    IHasher hasher,
    JwtTokenGenerator jwtGenerator
) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHasher _hasher = hasher;
    private readonly JwtTokenGenerator _jwtGenerator = jwtGenerator;

    public async Task<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>> LoginAsync(UserSignInDTO userDTO)
    {
        User user = await _userRepository.GetUserByUsernameAsync(userDTO.Username);

        if (!_hasher.VerifyPassword(userDTO.Password, user.Password))
        {
            throw new UnauthorizedActionException();
        }

        JWTTokenResDTO tokenDTO = _jwtGenerator.GenerateToken(user);
        return new ResponseDTO<JWTTokenResDTO?, ErrorDTO?>
        {
            Success = true,
            Data = tokenDTO,
            Message = "Success in login",
        };
    }

    public async Task<ResponseDTO<UserCreatedDTO>> SignupAsync(UserCreateDTO userDTO)
    {
        try { }
        catch { }
    }
}
