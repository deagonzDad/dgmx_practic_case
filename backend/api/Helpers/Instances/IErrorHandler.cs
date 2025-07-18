using api.DTO.Interfaces;
using api.DTO.ResponseDTO;
using api.Exceptions;

namespace api.Helpers.Instances;

public interface IErrorHandler
{
    public ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        ResponseDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        int Code
    )
        where TData : IResponseData?;
}
