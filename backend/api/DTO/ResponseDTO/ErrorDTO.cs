using System;

namespace api.DTO.ResponseDTO;

public class ErrorDTO
{
    public required string ErrorCode { get; set; }
    public required string ErrorDescription { get; set; }
    public required string ErrorDetail { get; set; }
}
