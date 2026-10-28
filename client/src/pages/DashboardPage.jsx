import { useEffect, useState } from 'react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../api'
import { rupiah, shortDate } from '../format'

export default function DashboardPage() {
  const [data, setData] = useState(null)
  const [days, setDays] = useState(7)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get(`/dashboard?days=${days}`).then(setData).catch((e) => setError(e.message))
  }, [days])

  if (error) return <div className="alert">{error}</div>
  if (!data) return <p>Memuat...</p>

  return (
    <>
      <h1>Dashboard</h1>

      <div className="stats">
        <Stat label="Penjualan Hari Ini" value={rupiah(data.todayTotal)} note={`${data.todayTransactions} transaksi`} />
        <Stat label="Penjualan Bulan Ini" value={rupiah(data.monthTotal)} note={`${data.monthTransactions} transaksi`} />
        <Stat label="Jumlah Produk" value={data.productCount} />
        <Stat label="Jumlah Pelanggan" value={data.customerCount} />
      </div>

      <div className="card">
        <div className="card-header">
          <h3>Penjualan per Hari</h3>
          <select value={days} onChange={(e) => setDays(Number(e.target.value))}>
            <option value={7}>7 hari terakhir</option>
            <option value={14}>14 hari terakhir</option>
            <option value={30}>30 hari terakhir</option>
          </select>
        </div>
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={data.dailySales}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} />
            <XAxis dataKey="date" tickFormatter={shortDate} />
            <YAxis tickFormatter={(v) => (v >= 1000 ? `${v / 1000}rb` : v)} width={60} />
            <Tooltip
              labelFormatter={shortDate}
              formatter={(v, _name, item) => [`${rupiah(v)} (${item.payload.transactionCount} trx)`, 'Total']}
            />
            <Bar dataKey="total" fill="#2563eb" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>

      <div className="grid-2">
        <div className="card">
          <h3>Produk Terlaris Bulan Ini</h3>
          <table>
            <thead>
              <tr><th>Produk</th><th className="num">Terjual</th><th className="num">Pendapatan</th></tr>
            </thead>
            <tbody>
              {data.topProducts.map((p) => (
                <tr key={p.productId}>
                  <td>{p.productName}</td>
                  <td className="num">{p.quantitySold}</td>
                  <td className="num">{rupiah(p.revenue)}</td>
                </tr>
              ))}
              {data.topProducts.length === 0 && <tr><td colSpan={3} className="muted">Belum ada penjualan</td></tr>}
            </tbody>
          </table>
        </div>

        <div className="card">
          <h3>Stok Menipis (≤ 10)</h3>
          <table>
            <thead>
              <tr><th>Kode</th><th>Produk</th><th className="num">Stok</th></tr>
            </thead>
            <tbody>
              {data.lowStock.map((p) => (
                <tr key={p.id}>
                  <td>{p.code}</td>
                  <td>{p.name}</td>
                  <td className={`num ${p.stock === 0 ? 'danger' : 'warning'}`}>{p.stock}</td>
                </tr>
              ))}
              {data.lowStock.length === 0 && <tr><td colSpan={3} className="muted">Semua stok aman</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

function Stat({ label, value, note }) {
  return (
    <div className="card stat">
      <div className="muted">{label}</div>
      <div className="stat-value">{value}</div>
      {note && <div className="muted small">{note}</div>}
    </div>
  )
}
