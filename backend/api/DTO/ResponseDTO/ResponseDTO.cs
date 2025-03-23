using System;
using System.Text.Json.Serialization;
using api.DTO.Interfaces;

namespace api.DTO.ResponseDTO;

public class ResponseDTO<T, D>
    where T : IResponseData?
{
    public bool Success { get; set; } = false;
    public int Code { get; set; }
    public required string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } = default;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public D? Error { get; set; } = default;
}
