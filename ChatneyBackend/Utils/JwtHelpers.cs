using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChatneyBackend.Utils;

public static class JwtHelpers
{
    public static string GetJwtToken(string email, string id, string jwtSalt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSalt));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Email, email),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static ClaimsPrincipal? ValidateJwtToken(string token, string jwtSalt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSalt);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false, 
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                ))
            {
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

}