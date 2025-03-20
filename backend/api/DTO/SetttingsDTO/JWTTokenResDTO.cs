using System;

namespace api.DTO.SetttingsDTO;

public class JWTTokenResDTO
{
    public required string Token { get; set; }
    public required string ExpireIn { get; set; }
    public required string RefreshToken { get; set; }
    public required string RefreshExpireIn { get; set; }
}
