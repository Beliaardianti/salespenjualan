import Modal from '../components/Modal'
import { dateTime, rupiah } from '../format'

// Struk / detail transaksi — dipakai di halaman Transaksi Baru & Riwayat
export default function SaleDetailModal({ sale, title = 'Detail Transaksi', onClose }) {
  return (
    <Modal title={title} onClose={onClose}>
      <div className="receipt">
        <div><strong>{sale.invoiceNumber}</strong></div>
        <div className="muted small">{dateTime(sale.saleDate)}</div>
        <div className="small">Pelanggan: {sale.customerName} &nbsp;•&nbsp; Kasir: {sale.cashierName}</div>
        <table>
          <thead>
            <tr><th>Produk</th><th className="num">Qty</th><th className="num">Harga</th><th className="num">Subtotal</th></tr>
          </thead>
          <tbody>
            {sale.items.map((i) => (
              <tr key={i.productId}>
                <td>{i.productName}</td>
                <td className="num">{i.quantity}</td>
                <td className="num">{rupiah(i.unitPrice)}</td>
                <td className="num">{rupiah(i.subtotal)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr><th colSpan={3}>Total</th><th className="num">{rupiah(sale.total)}</th></tr>
          </tfoot>
        </table>
      </div>
      <div className="modal-actions">
        <button className="btn btn-light" onClick={() => window.print()}>Cetak</button>
        <button className="btn btn-primary" onClick={onClose}>Tutup</button>
      </div>
    </Modal>
  )
}
