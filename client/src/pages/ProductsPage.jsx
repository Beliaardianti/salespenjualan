import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { useAuth } from '../auth'
import { rupiah } from '../format'
import Modal from '../components/Modal'

const emptyForm = { code: '', name: '', price: 0, stock: 0, categoryId: '' }

export default function ProductsPage() {
  const { isAdmin } = useAuth()
  const [products, setProducts] = useState([])
  const [categories, setCategories] = useState([])
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [editing, setEditing] = useState(null) // null = modal tertutup, { id?, ...form }
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')

  const load = () =>
    api.get(`/products${qs({ search, categoryId })}`).then(setProducts).catch((e) => setError(e.message))

  useEffect(() => {
    const t = setTimeout(load, 300)
    return () => clearTimeout(t)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, categoryId])

  useEffect(() => {
    api.get('/categories').then(setCategories).catch((e) => setError(e.message))
  }, [])

  const openForm = (p) => {
    setFormError('')
    setEditing(p ? { ...p } : { ...emptyForm })
  }

  const save = async (e) => {
    e.preventDefault()
    const body = { ...editing, price: Number(editing.price), stock: Number(editing.stock), categoryId: Number(editing.categoryId) }
    try {
      if (editing.id) await api.put(`/products/${editing.id}`, body)
      else await api.post('/products', body)
      setEditing(null)
      load()
    } catch (err) {
      setFormError(err.message)
    }
  }

  const remove = async (p) => {
    if (!confirm(`Hapus produk ${p.name}?`)) return
    try {
      await api.del(`/products/${p.id}`)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  const field = (key) => ({
    value: editing[key],
    onChange: (e) => setEditing({ ...editing, [key]: e.target.value }),
  })

  return (
    <>
      <div className="page-header">
        <h1>Produk</h1>
        {isAdmin && <button className="btn btn-primary" onClick={() => openForm(null)}>+ Tambah Produk</button>}
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <div className="toolbar">
          <input placeholder="Cari nama / kode..." value={search} onChange={(e) => setSearch(e.target.value)} />
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            <option value="">Semua kategori</option>
            {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>

        <table>
          <thead>
            <tr><th>Kode</th><th>Nama</th><th>Kategori</th><th className="num">Harga</th><th className="num">Stok</th>{isAdmin && <th></th>}</tr>
          </thead>
          <tbody>
            {products.map((p) => (
              <tr key={p.id}>
                <td>{p.code}</td>
                <td>{p.name}</td>
                <td>{p.categoryName}</td>
                <td className="num">{rupiah(p.price)}</td>
                <td className={`num ${p.stock === 0 ? 'danger' : p.stock <= 10 ? 'warning' : ''}`}>{p.stock}</td>
                {isAdmin && (
                  <td className="actions">
                    <button className="btn btn-light" onClick={() => openForm(p)}>Edit</button>
                    <button className="btn btn-danger" onClick={() => remove(p)}>Hapus</button>
                  </td>
                )}
              </tr>
            ))}
            {products.length === 0 && <tr><td colSpan={6} className="muted">Tidak ada produk</td></tr>}
          </tbody>
        </table>
      </div>

      {editing && (
        <Modal title={editing.id ? 'Edit Produk' : 'Tambah Produk'} onClose={() => setEditing(null)}>
          <form onSubmit={save} className="form">
            {formError && <div className="alert">{formError}</div>}
            <label>Kode <input {...field('code')} required maxLength={30} /></label>
            <label>Nama <input {...field('name')} required maxLength={150} /></label>
            <label>
              Kategori
              <select {...field('categoryId')} required>
                <option value="">-- pilih --</option>
                {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </label>
            <label>Harga (Rp) <input type="number" min={0} {...field('price')} required /></label>
            <label>Stok <input type="number" min={0} {...field('stock')} required /></label>
            <div className="modal-actions">
              <button type="button" className="btn btn-light" onClick={() => setEditing(null)}>Batal</button>
              <button className="btn btn-primary">Simpan</button>
            </div>
          </form>
        </Modal>
      )}
    </>
  )
}
