using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

[ApiController]
[Route("products")]
[Authorize]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET /products?search=kopi&categoryId=1
    [HttpGet]
    public async Task<List<ProductDto>> GetAll(string? search, int? categoryId)
    {
        // IQueryable = query yang belum dijalankan, bisa ditambah kondisi dulu
        // (mirip query builder Laravel: $q = Product::query(); if (...) $q->where(...))
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Code, pattern));
        }
        if (categoryId is not null)
            query = query.Where(p => p.CategoryId == categoryId);

        return await query.OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Code, p.Name, p.Price, p.Stock, p.CategoryId, p.Category!.Name))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var dto = await db.Products.Where(p => p.Id == id)
            .Select(p => new ProductDto(p.Id, p.Code, p.Name, p.Price, p.Stock, p.CategoryId, p.Category!.Name))
            .FirstOrDefaultAsync();
        return dto is null ? NotFound() : dto;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDto>> Create(ProductRequest req)
    {
        if (await Validate(req, null) is { } error) return error;

        var product = new Product();
        Apply(product, req);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return await GetById(product.Id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, ProductRequest req)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (await Validate(req, id) is { } error) return error;

        Apply(product, req);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (await db.SaleItems.AnyAsync(i => i.ProductId == id))
            return Conflict(new { message = "Produk sudah pernah terjual, tidak bisa dihapus" });

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult?> Validate(ProductRequest req, int? currentId)
    {
        var code = req.Code.Trim();
        if (await db.Products.AnyAsync(p => p.Code == code && p.Id != currentId))
            return Conflict(new { message = "Kode produk sudah dipakai" });
        if (!await db.Categories.AnyAsync(c => c.Id == req.CategoryId))
            return BadRequest(new { message = "Kategori tidak ditemukan" });
        return null;
    }

    private static void Apply(Product p, ProductRequest req)
    {
        p.Code = req.Code.Trim();
        p.Name = req.Name.Trim();
        p.Price = req.Price;
        p.Stock = req.Stock;
        p.CategoryId = req.CategoryId;
    }
}
