using System;
using api.DTO.UsersDTO;
using api.Helpers.Instances;
using api.Repository.Interfaces;
using api.Services.Interfaces;

namespace api.Services;

public class AuthService(IUserRepository userRepository, IHasher hasher) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IHasher _hasher = hasher;

    public async Task<ResponseDTO<>> LoginAsync(UserSignInDTO userDTO) { }

    public async Task<User> SignupAsync(UserCreateDTO userDTO) { }
}
