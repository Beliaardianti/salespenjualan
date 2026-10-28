import { useEffect, useState } from 'react'
import { api, qs } from '../api'
import { useAuth } from '../auth'
import { dateTime, rupiah, todayIso } from '../format'
import SaleDetailModal from './SaleDetailModal'

const PAGE_SIZE = 20

export default function SalesPage() {
  const { isAdmin } = useAuth()
  const [filter, setFilter] = useState({ from: todayIso(-30), to: todayIso(), search: '' })
  const [page, setPage] = useState(1)
  const [result, setResult] = useState(null)
  const [detail, setDetail] = useState(null)
  const [error, setError] = useState('')

  const load = () =>
    api.get(`/sales${qs({ ...filter, page, pageSize: PAGE_SIZE })}`)
      .then(setResult)
      .catch((e) => setError(e.message))

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => { load() }, [filter, page])

  const changeFilter = (key, value) => {
    setFilter((f) => ({ ...f, [key]: value }))
    setPage(1)
  }

  const showDetail = async (id) => {
    try {
      setDetail(await api.get(`/sales/${id}`))
    } catch (e) {
      setError(e.message)
    }
  }

  const cancel = async (sale) => {
    if (!confirm(`Batalkan ${sale.invoiceNumber}? Stok akan dikembalikan.`)) return
    try {
      await api.del(`/sales/${sale.id}`)
      load()
    } catch (e) {
      setError(e.message)
    }
  }

  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / PAGE_SIZE)) : 1
  const pageTotal = result?.items.reduce((s, x) => s + x.total, 0) ?? 0

  return (
    <>
      <h1>Riwayat Penjualan</h1>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <div className="toolbar">
          <label>Dari <input type="date" value={filter.from} onChange={(e) => changeFilter('from', e.target.value)} /></label>
          <label>Sampai <input type="date" value={filter.to} onChange={(e) => changeFilter('to', e.target.value)} /></label>
          <input placeholder="Cari invoice / pelanggan..." value={filter.search} onChange={(e) => changeFilter('search', e.target.value)} />
        </div>

        <table>
          <thead>
            <tr>
              <th>Invoice</th><th>Tanggal</th><th>Pelanggan</th><th>Kasir</th>
              <th className="num">Item</th><th className="num">Total</th><th></th>
            </tr>
          </thead>
          <tbody>
            {result?.items.map((s) => (
              <tr key={s.id}>
                <td>{s.invoiceNumber}</td>
                <td>{dateTime(s.saleDate)}</td>
                <td>{s.customerName}</td>
                <td>{s.cashierName}</td>
                <td className="num">{s.itemCount}</td>
                <td className="num">{rupiah(s.total)}</td>
                <td className="actions">
                  <button className="btn btn-light" onClick={() => showDetail(s.id)}>Detail</button>
                  {isAdmin && <button className="btn btn-danger" onClick={() => cancel(s)}>Batal</button>}
                </td>
              </tr>
            ))}
            {result?.items.length === 0 && <tr><td colSpan={7} className="muted">Tidak ada transaksi</td></tr>}
          </tbody>
          {result?.items.length > 0 && (
            <tfoot>
              <tr><th colSpan={5}>Total halaman ini</th><th className="num">{rupiah(pageTotal)}</th><th></th></tr>
            </tfoot>
          )}
        </table>

        <div className="pagination">
          <button className="btn btn-light" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹ Sebelumnya</button>
          <span>Halaman {page} / {totalPages} ({result?.totalCount ?? 0} transaksi)</span>
          <button className="btn btn-light" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>Berikutnya ›</button>
        </div>
      </div>

      {detail && <SaleDetailModal sale={detail} onClose={() => setDetail(null)} />}
    </>
  )
}
