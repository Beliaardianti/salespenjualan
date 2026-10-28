// Pembungkus fetch: otomatis kirim token JWT dan ubah error API jadi pesan yang mudah dibaca

const TOKEN_KEY = 'token'

export const getToken = () => localStorage.getItem(TOKEN_KEY)
export const setToken = (token) =>
  token ? localStorage.setItem(TOKEN_KEY, token) : localStorage.removeItem(TOKEN_KEY)

// Dipanggil saat token kadaluarsa (401) — diisi oleh AuthProvider
let onUnauthorized = () => {}
export const setUnauthorizedHandler = (fn) => (onUnauthorized = fn)

async function request(method, url, body) {
  const headers = { 'Content-Type': 'application/json' }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(`/api${url}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (res.status === 401 && token) onUnauthorized()

  // 204 No Content tidak punya body
  const data = res.status === 204 ? null : await res.json().catch(() => null)

  if (!res.ok) throw new Error(errorMessage(res, data))
  return data
}

// Format error dari ASP.NET Core:
//  - { message: "..." }                       → error buatan controller kita
//  - { errors: { Name: ["..."], ... } }       → error validasi otomatis [ApiController]
function errorMessage(res, data) {
  if (data?.message) return data.message
  if (data?.errors) return Object.values(data.errors).flat().join('\n')
  if (res.status === 401) return 'Sesi habis, silakan login ulang'
  if (res.status === 403) return 'Anda tidak punya akses untuk aksi ini'
  if (res.status === 404) return 'Data tidak ditemukan'
  return `Terjadi kesalahan (${res.status})`
}

export const api = {
  get: (url) => request('GET', url),
  post: (url, body) => request('POST', url, body),
  put: (url, body) => request('PUT', url, body),
  del: (url) => request('DELETE', url),
}

// Buat query string dari object, abaikan nilai kosong: { a: 1, b: '' } → "?a=1"
export function qs(params) {
  const entries = Object.entries(params).filter(([, v]) => v !== '' && v != null)
  return entries.length ? '?' + new URLSearchParams(entries).toString() : ''
}
