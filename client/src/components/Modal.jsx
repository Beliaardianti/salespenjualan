export default function Modal({ title, onClose, children }) {
  return (
    <div className="modal-backdrop" onClick={onClose}>
      {/* stopPropagation: klik di dalam modal tidak ikut menutup modal */}
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="btn-close" onClick={onClose}>×</button>
        </div>
        {children}
      </div>
    </div>
  )
}
