using System;
using api.DTO.Interfaces;

namespace api.DTO.SetttingsDTO;

public class JWTTokenResDTO : IResponseData
{
    public required string Token { get; set; }
    public required string ExpireIn { get; set; }
    public required string RefreshToken { get; set; }
    public required string RefreshExpireIn { get; set; }
}
