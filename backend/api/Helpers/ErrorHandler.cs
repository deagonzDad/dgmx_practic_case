using System;
using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.Helpers.Instances;

namespace api.Helpers;

public class ErrorHandler : IErrorHandler
{
    private string DetailError { get; set; } = string.Empty;
    private string ErrorCode { get; set; } = string.Empty;

    public ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        Exception ex,
        ResponseDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        string logMessage,
        int Code,
        ILogger? logger,
        bool isLogMessage = true
    )
        where TData : IResponseData?
    {
        if (isLogMessage)
        {
            logger?.LogError(ex, "{logMessage}", logMessage);
        }
        responseDTO.Success = false;
        responseDTO.Message = messageRes;
        responseDTO.Code = Code;
        responseDTO.Error = new ErrorDTO
        {
            ErrorCode = ErrorCode,
            ErrorDescription = messageRes,
            ErrorDetail = DetailError,
        };
        return responseDTO;
    }

    public void InitService(string errorDetail, string errorCode)
    {
        DetailError = errorDetail;
        ErrorCode = errorCode;
    }
}
