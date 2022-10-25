using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using zedbank.Core;
using zedbank.Database;
using zedbank.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace zedbank.Services;

public static class AuthService
{
    public static async Task<User?> Authenticate(UserAuthDto data, Context context)
    {
        try
        {
            var user = await context.Users.FirstAsync(u => u.Email == data.Email);
            if (user.CheckPassword(data.Password ?? string.Empty))
            {
                return user;
            }

            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static Token GenerateToken(User user)
    {
        var issuer = ConfigurationHelper.Config["Jwt:Issuer"]!;
        var key = Encoding.ASCII.GetBytes(ConfigurationHelper.Config["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(60 * 24),
            Issuer = issuer,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        return new Token
        {
            AccessToken = jwtToken,
        };
    }

    public static async Task<User> GetAuthUser(ClaimsPrincipal principal, Context context)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)!;
        var user = await context.Users.Include(u => u.Wallets)
            .Where(u => u.Id == long.Parse(sub.Value)).FirstAsync();
        return user!;
    }
}

public class Token
{
    public string AccessToken { get; set; } = null!;
}
