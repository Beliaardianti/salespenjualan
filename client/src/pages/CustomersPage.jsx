import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { useAuth } from '../auth'
import Modal from '../components/Modal'

const emptyForm = { name: '', phone: '', email: '', address: '' }

export default function CustomersPage() {
  const { isAdmin } = useAuth()
  const [customers, setCustomers] = useState([])
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState(null)
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')

  const load = () =>
    api.get(`/customers${qs({ search })}`).then(setCustomers).catch((e) => setError(e.message))

  useEffect(() => {
    const t = setTimeout(load, 300)
    return () => clearTimeout(t)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search])

  const openForm = (c) => {
    setFormError('')
    // null dari API → '' supaya input tetap "controlled"
    setEditing(c ? { ...c, phone: c.phone ?? '', email: c.email ?? '', address: c.address ?? '' } : { ...emptyForm })
  }

  const save = async (e) => {
    e.preventDefault()
    // field kosong dikirim sebagai null (validasi [EmailAddress] menolak string kosong)
    const body = {
      name: editing.name,
      phone: editing.phone || null,
      email: editing.email || null,
      address: editing.address || null,
    }
    try {
      if (editing.id) await api.put(`/customers/${editing.id}`, body)
      else await api.post('/customers', body)
      setEditing(null)
      load()
    } catch (err) {
      setFormError(err.message)
    }
  }

  const remove = async (c) => {
    if (!confirm(`Hapus pelanggan ${c.name}? Transaksi lamanya akan tercatat sebagai "Umum".`)) return
    try {
      await api.del(`/customers/${c.id}`)
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
        <h1>Pelanggan</h1>
        <button className="btn btn-primary" onClick={() => openForm(null)}>+ Tambah Pelanggan</button>
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <div className="toolbar">
          <input placeholder="Cari nama / no. HP..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </div>
        <table>
          <thead>
            <tr><th>Nama</th><th>No. HP</th><th>Email</th><th>Alamat</th><th></th></tr>
          </thead>
          <tbody>
            {customers.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.phone ?? '-'}</td>
                <td>{c.email ?? '-'}</td>
                <td>{c.address ?? '-'}</td>
                <td className="actions">
                  <button className="btn btn-light" onClick={() => openForm(c)}>Edit</button>
                  {isAdmin && <button className="btn btn-danger" onClick={() => remove(c)}>Hapus</button>}
                </td>
              </tr>
            ))}
            {customers.length === 0 && <tr><td colSpan={5} className="muted">Tidak ada pelanggan</td></tr>}
          </tbody>
        </table>
      </div>

      {editing && (
        <Modal title={editing.id ? 'Edit Pelanggan' : 'Tambah Pelanggan'} onClose={() => setEditing(null)}>
          <form onSubmit={save} className="form">
            {formError && <div className="alert">{formError}</div>}
            <label>Nama <input {...field('name')} required maxLength={100} /></label>
            <label>No. HP <input {...field('phone')} maxLength={30} /></label>
            <label>Email <input type="email" {...field('email')} maxLength={100} /></label>
            <label>Alamat <textarea {...field('address')} rows={3} /></label>
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
