import { useEffect, useState } from 'react'
import { api } from '../api'
import { useAuth } from '../auth'
import Modal from '../components/Modal'

const emptyForm = { username: '', password: '', fullName: '', role: 'Kasir' }

export default function UsersPage() {
  const { user: me } = useAuth()
  const [users, setUsers] = useState([])
  const [form, setForm] = useState(null)
  const [error, setError] = useState('')
  const [formError, setFormError] = useState('')

  const load = () => api.get('/users').then(setUsers).catch((e) => setError(e.message))
  useEffect(() => { load() }, [])

  const save = async (e) => {
    e.preventDefault()
    try {
      await api.post('/users', form)
      setForm(null)
      load()
    } catch (err) {
      setFormError(err.message)
    }
  }

  const remove = async (u) => {
    if (!confirm(`Hapus user ${u.username}?`)) return
    try {
      await api.del(`/users/${u.id}`)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  const field = (key) => ({
    value: form[key],
    onChange: (e) => setForm({ ...form, [key]: e.target.value }),
  })

  return (
    <>
      <div className="page-header">
        <h1>User</h1>
        <button className="btn btn-primary" onClick={() => { setFormError(''); setForm({ ...emptyForm }) }}>+ Tambah User</button>
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <table>
          <thead>
            <tr><th>Username</th><th>Nama</th><th>Role</th><th></th></tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>{u.username}</td>
                <td>{u.fullName}</td>
                <td><span className={`badge ${u.role === 'Admin' ? 'badge-blue' : ''}`}>{u.role}</span></td>
                <td className="actions">
                  {u.id !== me.id && <button className="btn btn-danger" onClick={() => remove(u)}>Hapus</button>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {form && (
        <Modal title="Tambah User" onClose={() => setForm(null)}>
          <form onSubmit={save} className="form">
            {formError && <div className="alert">{formError}</div>}
            <label>Username <input {...field('username')} required minLength={3} maxLength={50} /></label>
            <label>Password <input type="password" {...field('password')} required minLength={6} /></label>
            <label>Nama Lengkap <input {...field('fullName')} required maxLength={100} /></label>
            <label>
              Role
              <select {...field('role')}>
                <option value="Kasir">Kasir</option>
                <option value="Admin">Admin</option>
              </select>
            </label>
            <div className="modal-actions">
              <button type="button" className="btn btn-light" onClick={() => setForm(null)}>Batal</button>
              <button className="btn btn-primary">Simpan</button>
            </div>
          </form>
        </Modal>
      )}
    </>
  )
}
