using BelajarAspNetMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Data;

// Data awal — Laravel: DatabaseSeeder. Hanya jalan kalau tabel masih kosong.
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        if (!await db.Users.AnyAsync())
        {
            // Di server, isi password lewat environment variable Seed__AdminPassword & Seed__KasirPassword
            // supaya tidak memakai password demo yang mudah ditebak
            var adminPassword = config["Seed:AdminPassword"] ?? "admin123";
            var kasirPassword = config["Seed:KasirPassword"] ?? "kasir123";

            db.Users.AddRange(
                new User { Username = "admin", FullName = "Administrator", Role = Roles.Admin, PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword) },
                new User { Username = "kasir", FullName = "Kasir Satu", Role = Roles.Kasir, PasswordHash = BCrypt.Net.BCrypt.HashPassword(kasirPassword) });
        }

        if (!await db.Categories.AnyAsync())
        {
            var minuman = new Category { Name = "Minuman" };
            var makanan = new Category { Name = "Makanan" };
            var atk = new Category { Name = "Alat Tulis" };

            db.Products.AddRange(
                new Product { Code = "MNM-001", Name = "Kopi Susu", Price = 18_000, Stock = 50, Category = minuman },
                new Product { Code = "MNM-002", Name = "Teh Botol", Price = 6_000, Stock = 100, Category = minuman },
                new Product { Code = "MNM-003", Name = "Air Mineral 600ml", Price = 4_000, Stock = 8, Category = minuman },
                new Product { Code = "MKN-001", Name = "Roti Coklat", Price = 12_000, Stock = 30, Category = makanan },
                new Product { Code = "MKN-002", Name = "Keripik Kentang", Price = 10_000, Stock = 40, Category = makanan },
                new Product { Code = "ATK-001", Name = "Pulpen Hitam", Price = 3_500, Stock = 5, Category = atk },
                new Product { Code = "ATK-002", Name = "Buku Tulis 38 lbr", Price = 5_000, Stock = 60, Category = atk });
        }

        if (!await db.Customers.AnyAsync())
        {
            db.Customers.AddRange(
                new Customer { Name = "Budi Santoso", Phone = "081234567890", Email = "budi@example.com", Address = "Bandung" },
                new Customer { Name = "Siti Aminah", Phone = "081298765432", Address = "Jakarta" });
        }

        await db.SaveChangesAsync();
    }
}
