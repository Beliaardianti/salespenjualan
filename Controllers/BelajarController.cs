using Microsoft.AspNetCore.Mvc;
using BelajarAspNetMvc.Models;

namespace BelajarAspNetMvc.Controllers;

// ===== Pelajaran 1: Dasar C# untuk programmer JS/PHP =====
// Buka di browser: http://localhost:5258/api/belajar/pelajaran1
[ApiController]
[Route("api/[controller]")]
public class BelajarController : ControllerBase
{
    [HttpGet("pelajaran1")]
    public IActionResult Pelajaran1()
    {
        // Pengganti Console.WriteLine: kumpulkan baris teks, lalu tampilkan di browser
        var hasil = new List<string>();

        // 1. Variabel & tipe data
        string nama = "Budi";          // JS: let nama = "Budi";   PHP: $nama = "Budi";
        int umur = 25;                 // bilangan bulat
        decimal saldo = 150_000.50m;   // angka desimal untuk uang (akhiran m wajib)
        bool aktif = true;
        var kota = "Bandung";          // var = compiler menebak tipenya (string), tetap tidak bisa diganti tipe

        // 2. String interpolation — seperti `${}` di JS atau "{$x}" di PHP
        hasil.Add($"Halo, {nama}! Umur {umur}, tinggal di {kota}.");
        hasil.Add($"Saldo: Rp{saldo:N2}, aktif: {aktif}");

        // 3. if / else — sama persis dengan JS/PHP
        if (umur >= 17)
        {
            hasil.Add("Sudah boleh punya KTP.");
        }
        else
        {
            hasil.Add("Belum boleh punya KTP.");
        }

        // 4. List — seperti array di JS/PHP, tapi isinya harus satu tipe
        var buah = new List<string> { "Apel", "Jeruk", "Mangga" };
        buah.Add("Pisang");            // JS: buah.push("Pisang")   PHP: $buah[] = "Pisang"

        foreach (var b in buah)        // PHP: foreach ($buah as $b)   JS: for (const b of buah)
        {
            hasil.Add($"- {b}");
        }

        // 5. Memanggil method yang didefinisikan di bawah
        hasil.Add($"5 + 3 = {Tambah(5, 3)}");
        hasil.Add(Sapa("Siti"));

        // 6. Membuat objek dari class (lihat Models/Produk.cs)
        var produk = new Produk("Kopi", 25_000m);
        hasil.Add($"{produk.Nama} harganya Rp{produk.Harga:N0}");
        hasil.Add($"Setelah diskon 10%: Rp{produk.HargaDiskon(10):N0}");

        // Gabungkan semua baris jadi satu teks — PHP: implode("\n", $hasil)   JS: hasil.join("\n")
        return Content(string.Join("\n", hasil));
    }

    // ===== Pelajaran 2: Menerima input dari request =====

    // a) Route parameter — bagian dari URL
    // Buka: http://localhost:5258/api/belajar/halo/Budi
    // PHP (Laravel): Route::get('/halo/{nama}', ...)   JS (Express): app.get('/halo/:nama', ...)
    [HttpGet("halo/{nama}")]
    public IActionResult Halo(string nama)
    {
        return Content($"Halo, {nama}!");
    }

    // b) Route parameter bertipe — {angka:int} hanya cocok kalau isinya angka
    // Buka: http://localhost:5258/api/belajar/kuadrat/7   (coba juga /kuadrat/abc → 404)
    [HttpGet("kuadrat/{angka:int}")]
    public IActionResult Kuadrat(int angka)
    {
        // Ok(...) otomatis mengubah objek jadi JSON — seperti res.json() di Express / json_encode di PHP
        return Ok(new { angka, hasil = angka * angka });
    }

    // c) Query string — ?harga=...&diskon=...
    // Buka: http://localhost:5258/api/belajar/diskon?harga=50000&diskon=20
    // PHP: $_GET['harga']   JS (Express): req.query.harga
    [HttpGet("diskon")]
    public IActionResult Diskon(decimal harga, int diskon = 0)   // diskon opsional, default 0
    {
        if (harga <= 0)
        {
            // BadRequest = HTTP 400, seperti http_response_code(400) di PHP
            return BadRequest("Harga harus lebih dari 0");
        }

        var produk = new Produk("Barang", harga);
        return Ok(new { harga, diskon, hargaAkhir = produk.HargaDiskon(diskon) });
    }

    // d) Body JSON (POST) — data dikirim sebagai JSON, otomatis diubah jadi objek Produk
    // PHP: json_decode(file_get_contents('php://input'))   JS (Express): req.body
    // Coba di PowerShell:
    //   Invoke-RestMethod -Method Post -Uri http://localhost:5258/api/belajar/produk -ContentType "application/json" -Body '{"nama":"Teh","harga":8000}'
    [HttpPost("produk")]
    public IActionResult BuatProduk(Produk produk)
    {
        return Ok(new
        {
            pesan = $"Produk {produk.Nama} diterima",
            produk,
            hargaSetelahDiskon10 = produk.HargaDiskon(10),
        });
    }

    // Tipe parameter dan tipe hasil wajib ditulis
    private int Tambah(int a, int b)
    {
        return a + b;
    }

    // Versi singkat dengan => (mirip arrow function di JS)
    private string Sapa(string nama) => $"Selamat datang, {nama}!";
}
