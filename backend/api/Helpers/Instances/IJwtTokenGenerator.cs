using System;
using api.DTO.SettingsDTO;
using api.Models;

namespace api.Helpers.Instances;

public interface IJwtTokenGenerator
{
    JWTTokenResDTO GenerateToken(User user);
}
