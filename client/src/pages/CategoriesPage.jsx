import { useEffect, useState } from 'react'
import { api } from '../api'
import { useAuth } from '../auth'
import Modal from '../components/Modal'

export default function CategoriesPage() {
  const { isAdmin } = useAuth()
  const [categories, setCategories] = useState([])
  const [editing, setEditing] = useState(null)
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')

  const load = () => api.get('/categories').then(setCategories).catch((e) => setError(e.message))
  useEffect(() => { load() }, [])

  const openForm = (c) => {
    setFormError('')
    setEditing(c ? { ...c } : { name: '' })
  }

  const save = async (e) => {
    e.preventDefault()
    try {
      if (editing.id) await api.put(`/categories/${editing.id}`, { name: editing.name })
      else await api.post('/categories', { name: editing.name })
      setEditing(null)
      load()
    } catch (err) {
      setFormError(err.message)
    }
  }

  const remove = async (c) => {
    if (!confirm(`Hapus kategori ${c.name}?`)) return
    try {
      await api.del(`/categories/${c.id}`)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <>
      <div className="page-header">
        <h1>Kategori</h1>
        {isAdmin && <button className="btn btn-primary" onClick={() => openForm(null)}>+ Tambah Kategori</button>}
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <table>
          <thead>
            <tr><th>Nama</th><th className="num">Jumlah Produk</th>{isAdmin && <th></th>}</tr>
          </thead>
          <tbody>
            {categories.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td className="num">{c.productCount}</td>
                {isAdmin && (
                  <td className="actions">
                    <button className="btn btn-light" onClick={() => openForm(c)}>Edit</button>
                    <button className="btn btn-danger" onClick={() => remove(c)}>Hapus</button>
                  </td>
                )}
              </tr>
            ))}
            {categories.length === 0 && <tr><td colSpan={3} className="muted">Belum ada kategori</td></tr>}
          </tbody>
        </table>
      </div>

      {editing && (
        <Modal title={editing.id ? 'Edit Kategori' : 'Tambah Kategori'} onClose={() => setEditing(null)}>
          <form onSubmit={save} className="form">
            {formError && <div className="alert">{formError}</div>}
            <label>
              Nama
              <input value={editing.name} onChange={(e) => setEditing({ ...editing, name: e.target.value })} required maxLength={100} autoFocus />
            </label>
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
