using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.DTO.SetttingsDTO;
using api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace api.Helpers;

public class JwtTokenGenerator(IOptions<JwtSettingsDTO> jwtSettings)
{
    private readonly JwtSettingsDTO _jwtSettings = jwtSettings.Value;

    public string GenerateToken(User user)
    {
        List<string> listRoles = [.. user.Roles.Select(r => r.Name)];
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
        ];
        foreach (var role in listRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
