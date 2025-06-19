using System;
using System.Text.Json.Serialization;
using api.DTO.Interfaces;

namespace api.DTO.ResponseDTO;

public abstract class BaseAPIResponse<TError>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TError? Error { get; set; } = default;
}

public class ResponseDTO<T, D> : BaseAPIResponse<D>
    where T : IResponseData?
{
    public bool Success { get; set; } = false;
    public int Code { get; set; }
    public required string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } = default;
}

public class DataListPaginationDTO<T, D> : BaseAPIResponse<D>
    where T : IResponseData?
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<T?> Data { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TotalRecords { get; set; } = 0;
    public string? Next { get; set; } = null;
    public string? Previous { get; set; } = null;
}

public class FilterParamsDTO
{
    public int Limit { get; set; } = 10;
    public int SortOrder { get; set; } = 0;
    public string? Cursor { get; set; } = null;
    public string? SortBy { get; set; } = null;
    public string? Filter { get; set; } = null;
    public bool IsActive { get; set; } = true;
}
