using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

// CRUD Kategori — Laravel: Route::apiResource('categories', ...)
// Semua user yang login boleh melihat, hanya Admin yang boleh mengubah.
[ApiController]
[Route("categories")]
[Authorize]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<CategoryDto>> GetAll() =>
        await db.Categories.OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Products.Count))
            .ToListAsync();

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest req)
    {
        var name = req.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Name == name))
            return Conflict(new { message = "Nama kategori sudah ada" });

        var category = new Category { Name = name };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, 0);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, CategoryRequest req)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        var name = req.Name.Trim();
        if (await db.Categories.AnyAsync(c => c.Name == name && c.Id != id))
            return Conflict(new { message = "Nama kategori sudah ada" });

        category.Name = name;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        if (await db.Products.AnyAsync(p => p.CategoryId == id))
            return Conflict(new { message = "Kategori masih dipakai produk" });

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
