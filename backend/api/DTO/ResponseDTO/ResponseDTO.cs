using System;
using System.Text.Json.Serialization;

namespace api.DTO.ResponseDTO;

public class ResponseDTO<T, D>
{
    public bool Success { get; set; } = true;
    public required string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } = default;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public D? Error { get; set; } = default;
}
