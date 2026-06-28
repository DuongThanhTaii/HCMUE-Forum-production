import { useEffect } from 'react'

export function AzurePopupCompletePage() {
  useEffect(() => {
    const timeout = window.setTimeout(() => {
      if (window.opener && !window.opener.closed) {
        window.close()
      }
    }, 250)

    return () => window.clearTimeout(timeout)
  }, [])

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 p-4 text-center">
      <div className="max-w-sm rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <p className="text-sm font-medium text-slate-700">Microsoft login completed.</p>
        <p className="mt-2 text-xs text-slate-500">This popup will close automatically.</p>
      </div>
    </div>
  )
}
