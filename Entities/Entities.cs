namespace BelajarAspNetMvc.Entities;

// Entity = class yang dipetakan ke tabel database oleh EF Core
// (mirip Model Eloquent di Laravel). Satu properti = satu kolom.

public static class Roles
{
    public const string Admin = "Admin";
    public const string Kasir = "Kasir";
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // jangan pernah simpan password asli
    public string FullName { get; set; } = "";
    public string Role { get; set; } = Roles.Kasir;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    // Relasi one-to-many — Laravel: hasMany(Product::class)
    public List<Product> Products { get; set; } = [];
}

public class Product
{
    public int Id { get; set; }
    public string Code { get; set; } = "";    // SKU / kode barang, unik
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public int CategoryId { get; set; }       // foreign key
    public Category? Category { get; set; }   // Laravel: belongsTo(Category::class)
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }        // ? = boleh null
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class Sale
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime SaleDate { get; set; }    // selalu disimpan dalam UTC
    public decimal Total { get; set; }

    public int? CustomerId { get; set; }      // boleh kosong = pelanggan umum
    public Customer? Customer { get; set; }

    public int UserId { get; set; }           // kasir yang membuat transaksi
    public User? User { get; set; }

    public List<SaleItem> Items { get; set; } = [];
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }    // harga saat transaksi (harga produk bisa berubah nanti)
    public decimal Subtotal { get; set; }
}
