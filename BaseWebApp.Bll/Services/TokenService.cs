using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BaseWebApp.Bll.Interfaces;
using BaseWebApp.Dal.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BaseWebApp.Bll.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    
    //const string SHARED_SECRET_KEY = "This-Is-My-Super-Secure-And-Extra-Long-Dummy-Key-For-Testing-And-Debugging-1234567890!";

    public TokenService(IConfiguration config)
    {
        _config = config;
        // The key is created once and reused.
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    }

    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()) // "sub" is a standard claim for the subject (user) identifier
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // Token is valid for 7 days
            SigningCredentials = credentials,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}