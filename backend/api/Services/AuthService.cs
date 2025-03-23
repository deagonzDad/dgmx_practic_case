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

public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IHasher hasher,
    JwtTokenGenerator jwtGenerator,
    AppDbContext dbContext,
    IMapper mapper,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IHasher _hasher = hasher;
    private readonly JwtTokenGenerator _jwtGenerator = jwtGenerator;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AuthService> _logger = logger;

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
            return CreateErrorRes(
                ex,
                responseDTO,
                "Invalid username or password.",
                "User not found in the database",
                401
            );
        }
        catch (UnauthorizedActionException ex)
        {
            return CreateErrorRes(
                ex,
                responseDTO,
                "Invalid username or password.",
                "Password does not match the username",
                401
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
            (bool roleExist, List<Role> roles) = await _roleRepository.ValidateRolesExistAsync(
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
            return CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in user creation",
                400
            );
        }
        catch (RoleNotFoundException ex)
        {
            await transaction.RollbackAsync();
            return CreateErrorRes(
                ex,
                responseDTO,
                "Role not found.",
                "Role not found in the database",
                400
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return CreateErrorRes(
                ex,
                responseDTO,
                "something went wrong in the request",
                "An error occurred while processing your request.",
                500
            );
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    private ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        Exception ex,
        ResponseDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        string logMessage,
        int Code,
        bool isLogMessage = true
    )
        where TData : IResponseData?
    {
        if (isLogMessage)
        {
            _logger.LogError(ex, "{logMessage}", logMessage);
        }
        responseDTO.Success = false;
        responseDTO.Message = messageRes;
        responseDTO.Code = Code;
        responseDTO.Error = new ErrorDTO
        {
            ErrorCode = "USER",
            ErrorDescription = messageRes,
            ErrorDetail = "USER_ERROR",
        };
        return responseDTO;
    }
}
