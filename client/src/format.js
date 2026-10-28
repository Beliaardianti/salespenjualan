const rupiahFormatter = new Intl.NumberFormat('id-ID', {
  style: 'currency',
  currency: 'IDR',
  maximumFractionDigits: 0,
})

export const rupiah = (n) => rupiahFormatter.format(n ?? 0)

// Tanggal dari API dalam UTC (ISO string), tampilkan dalam waktu lokal
export const dateTime = (iso) =>
  new Date(iso).toLocaleString('id-ID', { dateStyle: 'medium', timeStyle: 'short' })

// "2026-10-28" → "28 Okt"
export const shortDate = (yyyyMmDd) =>
  new Date(yyyyMmDd + 'T00:00:00').toLocaleDateString('id-ID', { day: 'numeric', month: 'short' })

// Tanggal hari ini (lokal) format yyyy-mm-dd untuk <input type="date">
export function todayIso(offsetDays = 0) {
  const d = new Date()
  d.setDate(d.getDate() + offsetDays)
  const pad = (x) => String(x).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}
