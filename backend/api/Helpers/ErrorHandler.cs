using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.Exceptions;
using api.Helpers.Instances;

namespace api.Helpers;

public class ErrorHandler : IErrorHandler
{
    public ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        ResponseDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        int Code
    )
        where TData : IResponseData?
    {
        responseDTO.Success = false;
        responseDTO.Message = messageRes;
        responseDTO.Code = Code;
        responseDTO.Error = new ErrorDTO
        {
            ErrorCode = Code.ToString(),
            ErrorDescription = messageRes,
            ErrorDetail = messageRes,
        };
        return responseDTO;
    }
}
