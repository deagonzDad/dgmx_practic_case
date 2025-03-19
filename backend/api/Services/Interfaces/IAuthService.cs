using System;
using api.DTO.UsersDTO;
using api.Models;

namespace api.Services.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(UserSignInDTO userDTO);
    Task<User> SignupAsync(UserCreateDTO userDTO);
}
