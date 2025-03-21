using System;
using api.Data;
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
            User user = _mapper.Map<User>(userDTO);
            (bool roleExist, List<Role> roles) = await _roleRepository.ValidateRolesExistAsync(
                userDTO.Roles
            );
            if (!roleExist)
            {
                throw new RoleNotFoundException();
            }
            await _userRepository.CreateUserAsync(user);
            await _roleRepository.AssignRoleToUserAsync(user.Id, roles);
            await transaction.CommitAsync();
            responseDTO.Data = _mapper.Map<UserCreatedDTO>(user);
            responseDTO.Message = "Success in the user creation";
            responseDTO.Success = true;
            return responseDTO;
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            return CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "Database error in user creation"
            );
        }
        catch (RoleNotFoundException ex)
        {
            await transaction.RollbackAsync();
            return CreateErrorRes(ex, responseDTO, "Role not found.", "Role not found");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return CreateErrorRes(
                ex,
                responseDTO,
                "An error occurred while processing your request.",
                "something went wrong in the request"
            );
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    private ResponseDTO<UserCreatedDTO?, ErrorDTO?> CreateErrorRes(
        Exception ex,
        ResponseDTO<UserCreatedDTO?, ErrorDTO?> responseDTO,
        string messageRes,
        string logMessage
    )
    {
        _logger.LogError(ex, "{logMessage}", logMessage);
        responseDTO.Success = false;
        responseDTO.Message = messageRes;
        responseDTO.Data = null;
        responseDTO.Error = new ErrorDTO
        {
            ErrorCode = "USER",
            ErrorDescription = messageRes,
            ErrorDetail = "USER_ERROR",
        };
        return responseDTO;
    }
}
