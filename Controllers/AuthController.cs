using System.Security.Claims;
using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Entities;
using BelajarAspNetMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    // POST /auth/login  → { token, expiresAt, user }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

        // Cek password dengan BCrypt — PHP: password_verify()
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Username atau password salah" });

        var (token, expiresAt) = tokens.CreateToken(user);
        return new LoginResponse(token, expiresAt, ToDto(user));
    }

    // GET /auth/me → data user yang sedang login (dari token)
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(id);
        return user is null ? Unauthorized() : ToDto(user);
    }

    internal static UserDto ToDto(User u) => new(u.Id, u.Username, u.FullName, u.Role);
}
