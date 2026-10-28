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
[Route("sales")]
[Authorize]
public class SalesController(AppDbContext db, AppClock clock) : ControllerBase
{
    // GET /sales?from=2026-10-01&to=2026-10-31&search=INV&page=1&pageSize=20
    [HttpGet]
    public async Task<PagedResult<SaleListDto>> GetAll(
        DateOnly? from, DateOnly? to, string? search, int page = 1, int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Sales.AsQueryable();
        if (from is not null)
        {
            var fromUtc = clock.StartOfDayUtc(from.Value);
            query = query.Where(s => s.SaleDate >= fromUtc);
        }
        if (to is not null)
        {
            var toUtc = clock.StartOfDayUtc(to.Value.AddDays(1));   // sampai akhir hari "to"
            query = query.Where(s => s.SaleDate < toUtc);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.InvoiceNumber, pattern)
                                  || (s.Customer != null && EF.Functions.ILike(s.Customer.Name, pattern)));
        }

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new SaleListDto(
                s.Id, s.InvoiceNumber, s.SaleDate,
                s.Customer != null ? s.Customer.Name : "Umum",
                s.User!.FullName,
                s.Items.Sum(i => i.Quantity),
                s.Total))
            .ToListAsync();

        return new PagedResult<SaleListDto>(items, page, pageSize, total);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDetailDto>> GetById(int id)
    {
        var sale = await db.Sales.Where(s => s.Id == id)
            .Select(s => new SaleDetailDto(
                s.Id, s.InvoiceNumber, s.SaleDate, s.CustomerId,
                s.Customer != null ? s.Customer.Name : "Umum",
                s.User!.FullName,
                s.Total,
                s.Items.OrderBy(i => i.Id)
                    .Select(i => new SaleItemDto(i.ProductId, i.Product!.Code, i.Product.Name, i.Quantity, i.UnitPrice, i.Subtotal))
                    .ToList()))
            .FirstOrDefaultAsync();

        return sale is null ? NotFound() : sale;
    }

    // POST /sales  body: { customerId: 1, items: [{ productId: 1, quantity: 2 }] }
    [HttpPost]
    public async Task<ActionResult<SaleDetailDto>> Create(CreateSaleRequest req)
    {
        if (req.CustomerId is not null && !await db.Customers.AnyAsync(c => c.Id == req.CustomerId))
            return BadRequest(new { message = "Pelanggan tidak ditemukan" });

        // Gabungkan item dengan produk yang sama (misal produk 1 diinput dua kali)
        var lines = req.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity) })
            .ToList();

        var productIds = lines.Select(l => l.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        if (products.Count != lines.Count)
            return BadRequest(new { message = "Ada produk yang tidak ditemukan" });

        // Transaksi database: semua langkah berhasil, atau semuanya dibatalkan
        // Laravel: DB::transaction(function () { ... })
        await using var tx = await db.Database.BeginTransactionAsync();

        foreach (var line in lines)
        {
            // Kurangi stok langsung di database HANYA jika stok cukup.
            // Cara ini aman walaupun dua kasir checkout bersamaan.
            var updated = await db.Products
                .Where(p => p.Id == line.ProductId && p.Stock >= line.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - line.Quantity));

            if (updated == 0)
            {
                var p = products[line.ProductId];
                return BadRequest(new { message = $"Stok {p.Name} tidak cukup (sisa {p.Stock})" });
                // tx otomatis di-rollback karena tidak di-Commit
            }
        }

        var sale = new Sale
        {
            InvoiceNumber = Guid.NewGuid().ToString("N")[..20],   // sementara, diganti di bawah
            SaleDate = DateTime.UtcNow,
            CustomerId = req.CustomerId,
            UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Items = lines.Select(l => new SaleItem
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = products[l.ProductId].Price,
                Subtotal = products[l.ProductId].Price * l.Quantity,
            }).ToList(),
        };
        sale.Total = sale.Items.Sum(i => i.Subtotal);

        db.Sales.Add(sale);
        await db.SaveChangesAsync();

        // Nomor invoice memakai Id supaya pasti unik: INV-20261028-00012
        sale.InvoiceNumber = $"INV-{clock.ToLocal(sale.SaleDate):yyyyMMdd}-{sale.Id:D5}";
        await db.SaveChangesAsync();

        await tx.CommitAsync();

        var result = await GetById(sale.Id);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, result.Value);
    }

    // Batalkan transaksi (Admin): stok dikembalikan, transaksi dihapus
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Cancel(int id)
    {
        var sale = await db.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
        if (sale is null) return NotFound();

        await using var tx = await db.Database.BeginTransactionAsync();

        foreach (var item in sale.Items)
        {
            await db.Products.Where(p => p.Id == item.ProductId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + item.Quantity));
        }

        db.Sales.Remove(sale);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
}
