# Sistem Manajemen Penjualan

Backend **ASP.NET Core (.NET 10) + EF Core + PostgreSQL**, frontend **React (Vite)** di folder `client/`.

## Struktur

```
BelajarAspNetMvc/
├── Program.cs              # konfigurasi app: database, JWT, routing
├── appsettings*.json       # connection string & setting JWT
├── Entities/Entities.cs    # tabel database (User, Category, Product, Customer, Sale, SaleItem)
├── Data/
│   ├── AppDbContext.cs     # koneksi DB + aturan tabel
│   ├── DbSeeder.cs         # data awal (user, produk contoh)
│   └── Migrations/         # migration EF Core (otomatis dibuat)
├── Dtos/Dtos.cs            # bentuk data request/response API + validasi
├── Services/               # TokenService (JWT), AppClock (zona waktu WIB)
├── Controllers/            # endpoint API (/api/...)
└── client/                 # aplikasi React
    └── src/
        ├── api.js          # fetch + token JWT
        ├── auth.jsx        # state login
        ├── components/     # Layout (sidebar), Modal
        └── pages/          # Dashboard, Transaksi, Riwayat, Produk, Kategori, Pelanggan, User
```

## Cara menjalankan

1. **Atur password PostgreSQL** di `appsettings.Development.json`:
   ```json
   "Default": "Host=localhost;Port=5432;Database=sales_db;Username=postgres;Password=PASSWORD_ANDA"
   ```
   Database `sales_db` dibuat otomatis saat backend pertama kali jalan (beserta tabel & data contoh).

2. **Jalankan backend** (terminal 1, di folder ini):
   ```
   dotnet run
   ```
   API berjalan di http://localhost:5258

3. **Jalankan frontend** (terminal 2):
   ```
   cd client
   npm install      # hanya pertama kali
   npm run dev
   ```
   Buka http://localhost:5173

Akun demo: `admin` / `admin123` (Admin) dan `kasir` / `kasir123` (Kasir).

## Hak akses

| Fitur | Admin | Kasir |
|---|---|---|
| Dashboard, transaksi baru, riwayat | ✔ | ✔ |
| Lihat produk & kategori | ✔ | ✔ |
| Tambah/edit/hapus produk & kategori | ✔ | ✘ |
| Tambah/edit pelanggan | ✔ | ✔ |
| Hapus pelanggan, batalkan transaksi, kelola user | ✔ | ✘ |

## Endpoint API

Di backend, endpoint tidak memakai awalan `/api`. Frontend tetap memanggil `/api/...`, lalu proxy (Vite saat development, Vercel saat online) membuang awalan itu sebelum meneruskan ke backend.

Swagger: http://localhost:5258/swagger/index.html

| Method | URL | Keterangan |
|---|---|---|
| POST | `/auth/login` | login → token JWT |
| GET | `/auth/me` | user yang sedang login |
| GET/POST/PUT/DELETE | `/categories` | kategori |
| GET/POST/PUT/DELETE | `/products?search=&categoryId=` | produk |
| GET/POST/PUT/DELETE | `/customers?search=` | pelanggan |
| GET | `/sales?from=&to=&search=&page=` | riwayat penjualan |
| GET | `/sales/{id}` | detail transaksi |
| POST | `/sales` | buat transaksi (stok berkurang otomatis) |
| DELETE | `/sales/{id}` | batalkan transaksi (stok dikembalikan) |
| GET | `/dashboard?days=7` | ringkasan dashboard |
| GET/POST/DELETE | `/users` | kelola user (Admin) |

Dokumentasi OpenAPI (JSON): http://localhost:5258/openapi/v1.json

## Mengubah struktur database

Setelah mengubah `Entities/` atau `AppDbContext.cs` (mirip `php artisan make:migration`):

```
dotnet ef migrations add NamaPerubahan -o Data/Migrations
```

Migration diterapkan otomatis saat `dotnet run` (mode Development).
