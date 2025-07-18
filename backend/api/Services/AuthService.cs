using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.SettingsDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Helpers;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;

namespace api.Services;

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IHasher hasher,
    IJwtTokenGenerator jwtGenerator,
    AppDbContext dbContext,
    IMapper mapper
) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IHasher _hasher = hasher;
    private readonly IJwtTokenGenerator _jwtGenerator = jwtGenerator;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<ResponseDTO<JWTTokenResDTO?, ErrorDTO?>> LoginAsync(UserSignInDTO userDTO)
    {
        ResponseDTO<JWTTokenResDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };
        try
        {
            string userEmail =
                userDTO.Email ?? userDTO.Username ?? throw new UserNotFoundException(null);
            User user = await _userRepository.GetUserByEmailOrUsernameAsync(userEmail);
            if (!_hasher.VerifyPassword(userDTO.Password, user.Password))
            {
                throw new UnauthorizedActionException(null);
            }
            JWTTokenResDTO tokenDTO = _jwtGenerator.GenerateToken(user);
            responseDTO.Data = tokenDTO;
            responseDTO.Message = "Success in the login";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (UserNotFoundException)
        {
            throw new InvalidCredentialsException(null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ResponseDTO<UserCreatedDTO?, ErrorDTO?>> SignupAsync(UserCreateDTO userDTO)
    {
        ResponseDTO<UserCreatedDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = false,
            Message = "",
        };

        bool existingUser = await _userRepository.UsernameOrEmailExistsAsync(
            userDTO.Email,
            userDTO.Username
        );
        if (existingUser)
        {
            throw new AlreadyExistException(null);
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            userDTO.Password = _hasher.HashPassword(userDTO.Password);
            User user = _mapper.Map<User>(userDTO);
            List<Role> roles = await _roleRepository.GetRolesByIdAsync(userDTO.Roles);
            user.Roles = roles;
            await _userRepository.CreateUserAsync(user);
            await transaction.CommitAsync();
            responseDTO.Data = _mapper.Map<UserCreatedDTO>(user);
            responseDTO.Message = "Success in the user creation";
            responseDTO.Success = true;
            responseDTO.Code = 201;
            return responseDTO;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
