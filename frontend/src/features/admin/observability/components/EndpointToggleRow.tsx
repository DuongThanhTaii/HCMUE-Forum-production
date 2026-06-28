import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { AlertTriangle, Info } from 'lucide-react'
import type { EndpointToggleDto } from '../../types/admin.types'

interface EndpointToggleRowProps {
  toggle: EndpointToggleDto
  isSubmitting: boolean
  onSubmit: (endpointKey: string, isEnabled: boolean, reason: string | null) => Promise<void>
}

export function EndpointToggleRow({ toggle, isSubmitting, onSubmit }: EndpointToggleRowProps) {
  const { t } = useTranslation()
  const [isConfirmOpen, setIsConfirmOpen] = useState(false)
  const [reason, setReason] = useState('')
  const [isReasonInvalid, setIsReasonInvalid] = useState(false)

  const disableWithReason = async () => {
    const trimmedReason = reason.trim()
    if (!trimmedReason) {
      setIsReasonInvalid(true)
      return
    }
    await onSubmit(toggle.endpointKey, false, trimmedReason)
    setReason('')
    setIsReasonInvalid(false)
    setIsConfirmOpen(false)
  }

  const enableToggle = async () => {
    await onSubmit(toggle.endpointKey, true, null)
  }

  return (
    <article className={`p-4 transition-colors ${toggle.isEnabled ? 'hover:bg-slate-50' : 'bg-amber-50/50 hover:bg-amber-50'}`}>
      <div className="flex items-center justify-between gap-4">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 
              className="font-mono text-[13px] font-semibold text-slate-900 truncate" 
              title={toggle.endpointKey}
            >
              {toggle.endpointKey}
            </h3>
            {!toggle.isEnabled && (
              <span className="inline-flex items-center rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-bold text-amber-800 uppercase tracking-wide">
                Disabled
              </span>
            )}
          </div>
          <div className="mt-1 flex items-center gap-3 text-[11px] text-slate-500">
            <span>Version: {toggle.version}</span>
            <span>&bull;</span>
            <span className="flex items-center gap-1" title="Last updated by">
              <Info className="h-3 w-3" />
              {toggle.updatedBy || 'System'}
            </span>
          </div>
        </div>

        <div className="flex-shrink-0">
          {toggle.isEnabled ? (
            <button
              type="button"
              role="switch"
              aria-checked={true}
              onClick={() => setIsConfirmOpen(!isConfirmOpen)}
              disabled={isSubmitting}
              className="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent bg-emerald-600 transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-emerald-600 focus:ring-offset-2 disabled:opacity-50"
            >
              <span className="pointer-events-none inline-block h-5 w-5 translate-x-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out" />
            </button>
          ) : (
            <button
              type="button"
              role="switch"
              aria-checked={false}
              onClick={() => void enableToggle()}
              disabled={isSubmitting}
              className="relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent bg-slate-200 transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary-600 focus:ring-offset-2 disabled:opacity-50"
            >
              <span className="pointer-events-none inline-block h-5 w-5 translate-x-0 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out" />
            </button>
          )}
        </div>
      </div>

      {!toggle.isEnabled && toggle.reason && (
        <div className="mt-3 flex items-start gap-2 rounded-md bg-amber-100/50 p-3 text-sm text-amber-900 border border-amber-200/50">
          <AlertTriangle className="mt-0.5 h-4 w-4 flex-shrink-0 text-amber-600" />
          <div>
            <p className="font-semibold text-amber-800">API Disabled</p>
            <p className="mt-0.5 text-amber-700/80">{toggle.reason}</p>
          </div>
        </div>
      )}

      {toggle.isEnabled && isConfirmOpen ? (
        <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 p-4 shadow-sm animate-in fade-in slide-in-from-top-2 duration-200">
          <div className="flex items-center gap-2 text-amber-800 mb-3">
            <AlertTriangle className="h-5 w-5" />
            <h4 className="font-bold">Xác nhận tắt API</h4>
          </div>
          <p className="text-sm text-amber-700 mb-3">
            Toàn bộ hệ thống sẽ bị chặn khi gọi đến endpoint này. Vui lòng ghi rõ lý do.
          </p>
          
          <div className="space-y-3">
            <textarea
              id={`disable-reason-${toggle.endpointKey}`}
              className="w-full rounded-md border border-amber-300 bg-white px-3 py-2 text-sm placeholder-slate-400 focus:border-amber-500 focus:outline-none focus:ring-1 focus:ring-amber-500"
              placeholder="Nhập lý do tắt API..."
              rows={2}
              value={reason}
              onChange={(event) => {
                setReason(event.target.value)
                if (event.target.value.trim()) setIsReasonInvalid(false)
              }}
            />
            {isReasonInvalid ? <p className="text-xs font-medium text-rose-600">{t('admin.togglesPage.row.reasonRequired')}</p> : null}
            
            <div className="flex gap-2 justify-end pt-1">
              <button
                type="button"
                className="rounded-md px-3 py-1.5 text-sm font-medium text-slate-600 hover:bg-amber-100 hover:text-slate-900 transition-colors"
                onClick={() => {
                  setIsConfirmOpen(false)
                  setReason('')
                  setIsReasonInvalid(false)
                }}
                disabled={isSubmitting}
              >
                {t('common.cancel')}
              </button>
              <button
                type="button"
                className="rounded-md bg-amber-600 px-4 py-1.5 text-sm font-medium text-white shadow-sm hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2 transition-colors disabled:opacity-50"
                onClick={() => void disableWithReason()}
                disabled={isSubmitting}
              >
                Xác nhận Tắt
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </article>
  )
}
