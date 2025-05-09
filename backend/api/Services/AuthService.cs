using System;
using api.Data;
using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.DTO.SetttingsDTO;
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
            User user = await _userRepository.GetUserByUsernameAsync(userDTO.Username);
            if (!_hasher.VerifyPassword(userDTO.Password, user.Password))
            {
                throw new UnauthorizedActionException();
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
                401,
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
                401,
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
            (bool roleExist, List<Role> roles) = await _roleRepository.ValidateRolesExistByIdAsync(
                userDTO.Roles
            );
            if (!roleExist)
            {
                throw new RoleNotFoundException();
            }
            await _userRepository.CreateUserAsync(user);
            await _roleRepository.AssignRoleToUserAsync(user, roles, true);
            await transaction.CommitAsync();
            responseDTO.Data = _mapper.Map<UserCreatedDTO>(user);
            responseDTO.Message = "Success in the user creation";
            responseDTO.Success = true;
            responseDTO.Code = 201;
            return responseDTO;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in user creation",
                400,
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
                400,
                _logger
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return _errorHandler.CreateErrorRes(
                ex,
                responseDTO,
                "something went wrong in the request",
                "An error occurred while processing your request.",
                500,
                _logger
            );
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }
}
