using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Core.Entities;
using JsonClaimValueTypes = Microsoft.IdentityModel.JsonWebTokens.JsonClaimValueTypes;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace WebAPI.Services;

public class TokenService
{
    /// <summary>
    /// This in NO way generates a secure token. It is completely void of all header/security information.
    /// </summary>
    /// <param name="vendorUser"></param>
    /// <returns></returns>
    public string GenerateToken(VendorUserEntity vendorUser)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, vendorUser.UserId.ToString()),
            new("vnd", vendorUser.VendorId.ToString()),
            new(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString()),
            new(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)now.AddHours(1)).ToUnixTimeSeconds().ToString()),
            new("vndroles", JsonSerializer.Serialize(new List<string>
            {
                "admin"
            }), JsonClaimValueTypes.JsonArray),
            new("profile", JsonSerializer.Serialize(new
            {
                firstName = vendorUser.User.FirstName,
                lastName = vendorUser.User.LastName,
                email = vendorUser.User.Email,
            }), JsonClaimValueTypes.Json)
        };
        var header = new JwtHeader();
        var token = new JwtSecurityToken(header, new JwtPayload(claims));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}