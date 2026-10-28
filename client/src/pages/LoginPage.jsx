import { useState } from 'react'
import { useAuth } from '../auth'

export default function LoginPage() {
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (e) => {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await login(username, password)
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-page">
      <form className="card login-card" onSubmit={submit}>
        <h2>Sistem Manajemen Penjualan</h2>
        <p className="muted">Silakan login untuk melanjutkan</p>
        {error && <div className="alert">{error}</div>}
        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} autoFocus required />
        </label>
        <label>
          Password
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        <button className="btn btn-primary" disabled={busy}>
          {busy ? 'Memproses...' : 'Login'}
        </button>
        <p className="muted small">Akun demo: admin / admin123 &nbsp;•&nbsp; kasir / kasir123</p>
      </form>
    </div>
  )
}
