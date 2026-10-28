import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { rupiah } from '../format'
import SaleDetailModal from './SaleDetailModal'

// Halaman kasir: pilih produk → masuk keranjang → simpan transaksi
export default function PosPage() {
  const [products, setProducts] = useState([])
  const [customers, setCustomers] = useState([])
  const [search, setSearch] = useState('')
  const [cart, setCart] = useState([]) // [{ product, quantity }]
  const [customerId, setCustomerId] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [savedSale, setSavedSale] = useState(null)

  const loadProducts = () =>
    api.get(`/products${qs({ search })}`).then(setProducts).catch((e) => setError(e.message))

  // Cari produk 300ms setelah user berhenti mengetik (debounce)
  useEffect(() => {
    const t = setTimeout(loadProducts, 300)
    return () => clearTimeout(t)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search])

  useEffect(() => {
    api.get('/customers').then(setCustomers).catch((e) => setError(e.message))
  }, [])

  const qtyInCart = (productId) => cart.find((c) => c.product.id === productId)?.quantity ?? 0

  const addToCart = (product) => {
    if (qtyInCart(product.id) >= product.stock) return
    setCart((prev) =>
      prev.some((c) => c.product.id === product.id)
        ? prev.map((c) => (c.product.id === product.id ? { ...c, quantity: c.quantity + 1 } : c))
        : [...prev, { product, quantity: 1 }],
    )
  }

  const setQty = (productId, quantity) =>
    setCart((prev) =>
      prev
        .map((c) => (c.product.id === productId ? { ...c, quantity: Math.min(quantity, c.product.stock) } : c))
        .filter((c) => c.quantity > 0),
    )

  const total = cart.reduce((sum, c) => sum + c.product.price * c.quantity, 0)

  const checkout = async () => {
    setError('')
    setBusy(true)
    try {
      const sale = await api.post('/sales', {
        customerId: customerId ? Number(customerId) : null,
        items: cart.map((c) => ({ productId: c.product.id, quantity: c.quantity })),
      })
      setSavedSale(sale)
      setCart([])
      setCustomerId('')
      loadProducts() // stok sudah berubah
    } catch (e) {
      setError(e.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <h1>Transaksi Baru</h1>
      {error && <div className="alert">{error}</div>}

      <div className="pos">
        <div className="card">
          <input placeholder="Cari nama / kode produk..." value={search} onChange={(e) => setSearch(e.target.value)} />
          <div className="product-grid">
            {products.map((p) => {
              const sisa = p.stock - qtyInCart(p.id)
              return (
                <button key={p.id} className="product-tile" disabled={sisa <= 0} onClick={() => addToCart(p)}>
                  <strong>{p.name}</strong>
                  <span className="muted small">{p.code}</span>
                  <span>{rupiah(p.price)}</span>
                  <span className={`small ${sisa <= 0 ? 'danger' : 'muted'}`}>Stok: {sisa}</span>
                </button>
              )
            })}
            {products.length === 0 && <p className="muted">Produk tidak ditemukan</p>}
          </div>
        </div>

        <div className="card cart">
          <h3>Keranjang</h3>
          <label>
            Pelanggan
            <select value={customerId} onChange={(e) => setCustomerId(e.target.value)}>
              <option value="">Umum</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </label>

          <table>
            <tbody>
              {cart.map((c) => (
                <tr key={c.product.id}>
                  <td>
                    {c.product.name}
                    <div className="muted small">{rupiah(c.product.price)}</div>
                  </td>
                  <td style={{ width: 80 }}>
                    <input
                      type="number"
                      min={0}
                      max={c.product.stock}
                      value={c.quantity}
                      onChange={(e) => e.target.value !== '' && setQty(c.product.id, Number(e.target.value))}
                    />
                  </td>
                  <td className="num">{rupiah(c.product.price * c.quantity)}</td>
                  <td>
                    <button className="btn-close" onClick={() => setQty(c.product.id, 0)}>×</button>
                  </td>
                </tr>
              ))}
              {cart.length === 0 && <tr><td className="muted">Klik produk untuk menambahkan</td></tr>}
            </tbody>
          </table>

          <div className="cart-total">
            <span>Total</span>
            <strong>{rupiah(total)}</strong>
          </div>
          <button className="btn btn-primary btn-block" disabled={cart.length === 0 || busy} onClick={checkout}>
            {busy ? 'Menyimpan...' : 'Simpan Transaksi'}
          </button>
        </div>
      </div>

      {savedSale && <SaleDetailModal sale={savedSale} title="Transaksi Berhasil" onClose={() => setSavedSale(null)} />}
    </>
  )
}
