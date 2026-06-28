import { AlertTriangle } from 'lucide-react'
import { Link } from 'react-router-dom'

export function MaintenancePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-xl rounded-2xl border border-amber-200 bg-white p-8 shadow-sm">
        <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-full bg-amber-100 text-amber-700">
          <AlertTriangle className="h-6 w-6" />
        </div>
        <h1 className="text-2xl font-bold text-slate-900">Hệ thống đang bảo trì</h1>
        <p className="mt-2 text-sm text-slate-600">
          UniHub đang tạm dừng để cập nhật. Vui lòng quay lại sau ít phút.
        </p>
        <div className="mt-6 flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-white hover:opacity-90"
          >
            Thử lại
          </button>
          <Link
            to="/login"
            className="rounded-md border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-100"
          >
            Về trang đăng nhập
          </Link>
        </div>
      </div>
    </div>
  )
}
