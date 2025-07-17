using System.Net;
using System.Text.Json;
using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Helpers.Instances;
using Microsoft.AspNetCore.Mvc;

namespace api.Infrastructure;

public class GlobalExceptionHandler(
    RequestDelegate next,
    ILogger<GlobalExceptionHandler> logger,
    IErrorHandler _errorHandler
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IErrorHandler _errorHandler = _errorHandler;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var errorResponse = new ResponseDTO<IResponseData?, ErrorDTO?>
        {
            Data = null,
            Success = false,
            Message = "An internal server error has occurred.",
        };
        bool isGoingLog = false;
        switch (exception)
        {
            case UnauthorizedActionException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "You are not authorized to perform this action.",
                    StatusCodes.Status401Unauthorized
                );
                break;
            case UserNotFoundException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "User Not Found",
                    StatusCodes.Status404NotFound
                );
                break;
            case RoomNotFoundException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "Room Not Found",
                    StatusCodes.Status404NotFound
                );
                break;
            case InvalidCredentialsException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "Invalid credentials",
                    StatusCodes.Status401Unauthorized
                );
                break;
            case ReservationNotFoundException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "Reservation Not Found",
                    StatusCodes.Status404NotFound
                );
                break;
            case UpdateException:
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "An error occurred while processing your request.",
                    StatusCodes.Status400BadRequest
                );
                break;
            default:
                isGoingLog = true;
                errorResponse = _errorHandler.CreateErrorRes(
                    errorResponse,
                    "An unexpected error occurred.",
                    StatusCodes.Status500InternalServerError
                );
                break;
        }
        if (isGoingLog)
            _logger.LogError(
                exception,
                "An unhandled exception has occurred: {Message}",
                exception.Message
            );
        context.Response.StatusCode = errorResponse.Code;
        var result = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(result);
    }
}
