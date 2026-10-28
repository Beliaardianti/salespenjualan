using System.Security.Claims;
using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

// Manajemen user — hanya Admin (Laravel: middleware('role:admin'))
[ApiController]
[Route("users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<UserDto>> GetAll() =>
        await db.Users.OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role))
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Username sudah dipakai" });

        var user = new User
        {
            Username = req.Username.Trim(),
            FullName = req.FullName.Trim(),
            Role = req.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return AuthController.ToDto(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier))
            return BadRequest(new { message = "Tidak bisa menghapus akun sendiri" });

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (await db.Sales.AnyAsync(s => s.UserId == id))
            return Conflict(new { message = "User sudah punya transaksi, tidak bisa dihapus" });

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
