using BelajarAspNetMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Data;

// DbContext = pintu ke database. Setiap DbSet = satu tabel.
// Laravel: kira-kira gabungan koneksi DB + semua Model.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    // Aturan tambahan tabel (index unik, panjang kolom, dll) — mirip isi file migration Laravel
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(50);
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(20);
        });

        b.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100);
        });

        b.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(30);
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Price).HasPrecision(18, 2);
            // Kategori tidak boleh dihapus kalau masih punya produk
            e.HasOne(x => x.Category).WithMany(c => c.Products).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Customer>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(100);
        });

        b.Entity<Sale>(e =>
        {
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasIndex(x => x.SaleDate);
            e.Property(x => x.InvoiceNumber).HasMaxLength(30);
            e.Property(x => x.Total).HasPrecision(18, 2);
            e.HasOne(x => x.Customer).WithMany().OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Items).WithOne(i => i.Sale).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SaleItem>(e =>
        {
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            // Produk yang sudah pernah terjual tidak boleh dihapus (riwayat harus tetap utuh)
            e.HasOne(x => x.Product).WithMany().OnDelete(DeleteBehavior.Restrict);
        });
    }
}
