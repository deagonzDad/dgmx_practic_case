using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Helpers.Instances;

namespace api.Helpers;

public class ErrorHandler : IErrorHandler
{
    private string DetailError { get; set; } = string.Empty;
    private string ErrorCode { get; set; } = string.Empty;

    public DataListPaginationDTO<TData, ErrorDTO?> CreateErrorListRes<TData>(
        BaseApiException ex,
        DataListPaginationDTO<TData, ErrorDTO?> responseDTO,
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
            logger?.LogError(ex.LogError, "{logMessage}", logMessage);
        }
        responseDTO.Error = new ErrorDTO
        {
            ErrorCode = ErrorCode,
            ErrorDescription = messageRes,
            ErrorDetail = DetailError,
            ApiErrorCode = Code,
        };
        return responseDTO;
    }

    public ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        BaseApiException ex,
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
            logger?.LogError(ex.LogError, "{logMessage}", logMessage);
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
