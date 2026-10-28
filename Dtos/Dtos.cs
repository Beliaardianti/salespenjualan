using System.ComponentModel.DataAnnotations;

namespace BelajarAspNetMvc.Dtos;

// DTO (Data Transfer Object) = bentuk data yang keluar/masuk API.
// Dipisah dari Entity supaya PasswordHash dll tidak ikut terkirim ke frontend,
// dan supaya validasi input jelas — mirip FormRequest + API Resource di Laravel.

// ===== Auth =====
public record LoginRequest([Required] string Username, [Required] string Password);
public record UserDto(int Id, string Username, string FullName, string Role);
public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public record CreateUserRequest(
    [Required, StringLength(50, MinimumLength = 3)] string Username,
    [Required, StringLength(100, MinimumLength = 6)] string Password,
    [Required, StringLength(100)] string FullName,
    [Required, RegularExpression("^(Admin|Kasir)$", ErrorMessage = "Role harus Admin atau Kasir")] string Role);

// ===== Kategori =====
public record CategoryDto(int Id, string Name, int ProductCount);
public record CategoryRequest([Required, StringLength(100)] string Name);

// ===== Produk =====
public record ProductDto(int Id, string Code, string Name, decimal Price, int Stock, int CategoryId, string CategoryName);
public record ProductRequest(
    [Required, StringLength(30)] string Code,
    [Required, StringLength(150)] string Name,
    [Range(0, 1_000_000_000)] decimal Price,
    [Range(0, int.MaxValue)] int Stock,
    [Range(1, int.MaxValue, ErrorMessage = "Kategori wajib dipilih")] int CategoryId);

// ===== Pelanggan =====
public record CustomerDto(int Id, string Name, string? Phone, string? Email, string? Address);
public record CustomerRequest(
    [Required, StringLength(100)] string Name,
    [StringLength(30)] string? Phone,
    [EmailAddress, StringLength(100)] string? Email,
    string? Address);

// ===== Penjualan =====
public record SaleItemRequest(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(1, 100_000)] int Quantity);
public record CreateSaleRequest(
    int? CustomerId,
    [Required, MinLength(1, ErrorMessage = "Minimal 1 item")] List<SaleItemRequest> Items);

public record SaleItemDto(int ProductId, string ProductCode, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);
public record SaleListDto(int Id, string InvoiceNumber, DateTime SaleDate, string CustomerName, string CashierName, int ItemCount, decimal Total);
public record SaleDetailDto(int Id, string InvoiceNumber, DateTime SaleDate, int? CustomerId, string CustomerName, string CashierName, decimal Total, List<SaleItemDto> Items);

// Hasil list dengan paging — Laravel: paginate()
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

// ===== Dashboard =====
public record DailySalesDto(DateOnly Date, int TransactionCount, decimal Total);
public record TopProductDto(int ProductId, string ProductName, int QuantitySold, decimal Revenue);
public record LowStockDto(int Id, string Code, string Name, int Stock);
public record DashboardDto(
    decimal TodayTotal,
    int TodayTransactions,
    decimal MonthTotal,
    int MonthTransactions,
    int ProductCount,
    int CustomerCount,
    List<DailySalesDto> DailySales,
    List<TopProductDto> TopProducts,
    List<LowStockDto> LowStock);
