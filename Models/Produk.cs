namespace BelajarAspNetMvc.Models;

// record = class ringkas; constructor dan properti Nama & Harga dibuat otomatis
public record Produk(string Nama, decimal Harga)
{
    public decimal HargaDiskon(int persen) => Harga - (Harga * persen / 100);
}
