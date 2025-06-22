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
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IHasher _hasher;
    private readonly JwtTokenGenerator _jwtGenerator;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly IErrorHandler _errorHandler;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IHasher hasher,
        JwtTokenGenerator jwtGenerator,
        AppDbContext dbContext,
        IMapper mapper,
        ILogger<AuthService> logger,
        IErrorHandler errorHandler
    )
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _hasher = hasher;
        _jwtGenerator = jwtGenerator;
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
        _errorHandler = errorHandler;
        _errorHandler.InitService("USER", "USER_ERROR");
    }

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
        catch (UserNotFoundException ex)
        {
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "Invalid username or password.",
                "User not found in the database",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
        catch (UnauthorizedActionException ex)
        {
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "Invalid username or password.",
                "Password does not match the username",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
    }

    public async Task<ResponseDTO<UserCreatedDTO?, ErrorDTO?>> SignupAsync(UserCreateDTO userDTO)
    {
        ResponseDTO<UserCreatedDTO?, ErrorDTO?> responseDTO = new()
        {
            Success = true,
            Message = "",
        };
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
        catch (UpdateException ex)
        {
            await transaction.RollbackAsync();
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in user creation",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
        catch (RoleNotFoundException ex)
        {
            await transaction.RollbackAsync();
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "Role not found.",
                "Role not found in the database",
                StatusCodes.Status400BadRequest,
                _logger
            );
        }
        // catch (Exception ex)
        // {
        //     await transaction.RollbackAsync();
        //     return _errorHandler.CreateErrorRes(
        //         ex,
        //         responseDTO,
        //         "something went wrong in the request",
        //         "An error occurred while processing your request.",
        //         StatusCodes.Status500InternalServerError,
        //         _logger
        //     );
        // }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
