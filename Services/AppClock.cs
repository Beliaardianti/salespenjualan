namespace BelajarAspNetMvc.Services;

// Database menyimpan waktu dalam UTC, tapi "hari ini" untuk toko = waktu lokal (WIB).
// Class ini membantu konversi UTC <-> waktu lokal toko.
public class AppClock(IConfiguration config)
{
    private readonly TimeZoneInfo _zone =
        TimeZoneInfo.FindSystemTimeZoneById(config["App:TimeZone"] ?? "Asia/Jakarta");

    public DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, _zone);

    public DateOnly Today => DateOnly.FromDateTime(ToLocal(DateTime.UtcNow));

    // Awal suatu tanggal lokal (jam 00:00 WIB), dikonversi ke UTC untuk query database
    public DateTime StartOfDayUtc(DateOnly date) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), _zone);
}
