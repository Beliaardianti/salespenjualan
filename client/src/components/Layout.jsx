import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth'

export default function Layout() {
  const { user, isAdmin, logout } = useAuth()

  const menu = [
    { to: '/', label: 'Dashboard' },
    { to: '/pos', label: 'Transaksi Baru' },
    { to: '/sales', label: 'Riwayat Penjualan' },
    { to: '/products', label: 'Produk' },
    { to: '/categories', label: 'Kategori' },
    { to: '/customers', label: 'Pelanggan' },
    ...(isAdmin ? [{ to: '/users', label: 'User' }] : []),
  ]

  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="brand">Sales App</div>
        <nav>
          {menu.map((m) => (
            <NavLink key={m.to} to={m.to} end={m.to === '/'}>
              {m.label}
            </NavLink>
          ))}
        </nav>
        <div className="user-box">
          <div>{user.fullName}</div>
          <small>{user.role}</small>
          <button className="btn btn-light" onClick={logout}>Logout</button>
        </div>
      </aside>
      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
