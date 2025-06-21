using api.DTO.Interfaces;
using api.DTO.ResponseDTO;

namespace api.Helpers.Instances;

public interface IErrorHandler
{
    void InitService(string errorDetail, string errorCode);
    public ResponseDTO<TData, ErrorDTO?> CreateErrorRes<TData>(
        Exception ex,
        ResponseDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        string logMessage,
        int Code,
        ILogger? logger,
        bool isLogMessage = true
    )
        where TData : IResponseData?;
    public DataListPaginationDTO<TData, ErrorDTO?> CreateErrorListRes<TData>(
        Exception ex,
        DataListPaginationDTO<TData, ErrorDTO?> responseDTO,
        string messageRes,
        string logMessage,
        int Code,
        ILogger? logger,
        bool isLogMessage = true
    )
        where TData : IResponseData?;
}
