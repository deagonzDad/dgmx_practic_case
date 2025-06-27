using System;
using api.Data;
using api.DTO.ResponseDTO;
using api.DTO.UsersDTO;
using api.Exceptions;
using api.Helpers.Instances;
using api.Models;
using api.Repository.Interfaces;
using api.Services.Interfaces;
using AutoMapper;

namespace api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IErrorHandler _errorHandler;
    private readonly IUserRepository _userRepository;

    public UserService(
        AppDbContext context,
        IMapper mapper,
        ILogger<UserService> logger,
        IErrorHandler errorHandler,
        IUserRepository userRepository
    )
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _errorHandler = errorHandler;
        _userRepository = userRepository;
        errorHandler.InitService("USER", "USER_ERROR");
    }

    public async Task<DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?>> GetUsersAsync(
        FilterParamsDTO filterParams
    )
    {
        DataListPaginationDTO<UserCreatedDTO?, ErrorDTO?> responseDTO = new() { };
        try
        {
            (List<User> query, int? nextCursor, int totalCount) =
                await _userRepository.GetUsersAsync(filterParams);
            List<UserCreatedDTO> userMap = _mapper.Map<List<UserCreatedDTO>>(query);
            responseDTO.Data = userMap!;
            responseDTO.Next = nextCursor.ToString();
            responseDTO.Previous = filterParams.Cursor;
            responseDTO.TotalRecords = totalCount;
            return responseDTO;
        }
        catch (UserNotFoundException ex)
        {
            return _errorHandler.CreateErrorListRes(
                ex,
                responseDTO,
                "User not found.",
                "User not found in the database",
                StatusCodes.Status404NotFound,
                _logger
            );
        }
    }
}
