using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BelajarAspNetMvc.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BelajarAspNetMvc.Services;

// Pengaturan JWT dari appsettings.json bagian "Jwt"
public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 480;
}

// Membuat token JWT setelah login berhasil — mirip Laravel Sanctum/JWT-auth
public class TokenService(JwtOptions options)
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);

        // Claim = data yang "ditempel" di token (siapa user-nya, role-nya)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("fullName", user.FullName),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
