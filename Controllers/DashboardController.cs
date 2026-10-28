using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

[ApiController]
[Route("dashboard")]
[Authorize]
public class DashboardController(AppDbContext db, AppClock clock) : ControllerBase
{
    // GET /dashboard?days=7&lowStockThreshold=10
    [HttpGet]
    public async Task<DashboardDto> Get(int days = 7, int lowStockThreshold = 10)
    {
        days = Math.Clamp(days, 1, 90);

        var today = clock.Today;
        var todayStart = clock.StartOfDayUtc(today);
        var monthStart = clock.StartOfDayUtc(new DateOnly(today.Year, today.Month, 1));
        var rangeStart = clock.StartOfDayUtc(today.AddDays(-(days - 1)));

        var todaySales = db.Sales.Where(s => s.SaleDate >= todayStart);
        var monthSales = db.Sales.Where(s => s.SaleDate >= monthStart);

        // Penjualan harian: ambil data periode ini lalu kelompokkan per tanggal WIB di C#
        // (dikelompokkan di C# karena tanggal lokal ≠ tanggal UTC di database)
        var rangeSales = await db.Sales.Where(s => s.SaleDate >= rangeStart)
            .Select(s => new { s.SaleDate, s.Total })
            .ToListAsync();

        var byDate = rangeSales
            .GroupBy(s => DateOnly.FromDateTime(clock.ToLocal(s.SaleDate)))
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Total: g.Sum(x => x.Total)));

        // Isi semua tanggal, termasuk yang tidak ada penjualan (nilai 0), supaya grafik rapi
        var daily = Enumerable.Range(0, days)
            .Select(i => today.AddDays(-(days - 1) + i))
            .Select(d => byDate.TryGetValue(d, out var v)
                ? new DailySalesDto(d, v.Count, v.Total)
                : new DailySalesDto(d, 0, 0))
            .ToList();

        // Produk terlaris bulan ini.
        // Dua langkah: (1) jumlahkan per ProductId di database, (2) ambil nama produknya.
        // EF Core tidak bisa menerjemahkan GroupBy + relasi + OrderBy pada DTO dalam satu query.
        var topSold = await db.SaleItems
            .Where(i => i.Sale!.SaleDate >= monthStart)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Subtotal) })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync();

        var topIds = topSold.Select(x => x.ProductId).ToList();
        var names = await db.Products.Where(p => topIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);
        var topProducts = topSold
            .Select(x => new TopProductDto(x.ProductId, names[x.ProductId], x.Quantity, x.Revenue))
            .ToList();

        var lowStock = await db.Products
            .Where(p => p.Stock <= lowStockThreshold)
            .OrderBy(p => p.Stock)
            .Select(p => new LowStockDto(p.Id, p.Code, p.Name, p.Stock))
            .ToListAsync();

        return new DashboardDto(
            TodayTotal: await todaySales.SumAsync(s => (decimal?)s.Total) ?? 0,
            TodayTransactions: await todaySales.CountAsync(),
            MonthTotal: await monthSales.SumAsync(s => (decimal?)s.Total) ?? 0,
            MonthTransactions: await monthSales.CountAsync(),
            ProductCount: await db.Products.CountAsync(),
            CustomerCount: await db.Customers.CountAsync(),
            DailySales: daily,
            TopProducts: topProducts,
            LowStock: lowStock);
    }
}
