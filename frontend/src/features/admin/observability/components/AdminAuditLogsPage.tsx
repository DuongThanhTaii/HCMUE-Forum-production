import { LogsFilterBar } from './LogsFilterBar'
import { useAdminLogsPage } from '../hooks/useAdminLogsPage'
import { useGetUsersQuery } from '../../api/admin.api'

export function AdminAuditLogsPage() {
  const {
    t,
    auditLogs,
    isAuditLogsLoading,
    isAuditLogsError,
    auditUserId,
    setAuditUserId,
    auditEndpointKey,
    setAuditEndpointKey,
    auditResultFilter,
    setAuditResultFilter,
    auditTake,
    setAuditTake,
  } = useAdminLogsPage()
  const { data: usersData } = useGetUsersQuery()
  const users = usersData ?? []

  const exportJson = () => {
    const payload = JSON.stringify(auditLogs, null, 2)
    const blob = new Blob([payload], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `audit-logs-${auditUserId || 'unknown'}-${new Date().toISOString()}.json`
    a.click()
    URL.revokeObjectURL(url)
  }

  const copyLine = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
    } catch {
      // no-op
    }
  }

  if (isAuditLogsLoading) {
    return (
      <div className="rounded-xl border border-slate-800 bg-slate-900 p-4 font-mono text-green-400">
        <div className="flex items-center gap-2">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-slate-600 border-b-green-400" />
          <span>Loading terminal logs...</span>
        </div>
      </div>
    )
  }

  if (isAuditLogsError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">
        {t('admin.auditLogsPage.messages.loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header className="rounded-xl border border-slate-200 bg-white p-4">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h1 className="text-lg font-semibold text-slate-900">{t('admin.auditLogsPage.title')}</h1>
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-500">Auto-refresh: 5s</span>
            <button
              type="button"
              onClick={exportJson}
              disabled={!auditLogs.length}
              className="rounded-md border border-slate-300 px-2.5 py-1 text-xs text-slate-700 disabled:opacity-50"
            >
              Export JSON
            </button>
          </div>
        </div>
      </header>

      <LogsFilterBar>
        <label className="text-sm font-medium text-slate-700">
          Search User
          <input
            list="audit-users-list"
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            placeholder="Select or type User ID..."
            value={auditUserId}
            onChange={(event) => setAuditUserId(event.target.value)}
          />
          <datalist id="audit-users-list">
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} ({u.email})
              </option>
            ))}
          </datalist>
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.auditLogsPage.filters.endpointKey')}
          <input className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm" value={auditEndpointKey} onChange={(event) => setAuditEndpointKey(event.target.value)} />
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.auditLogsPage.filters.result')}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={auditResultFilter}
            onChange={(event) => setAuditResultFilter(event.target.value as 'all' | 'success' | 'failure')}
          >
            <option value="all">{t('admin.auditLogsPage.filters.resultOptions.all')}</option>
            <option value="success">{t('admin.auditLogsPage.filters.resultOptions.success')}</option>
            <option value="failure">{t('admin.auditLogsPage.filters.resultOptions.failure')}</option>
          </select>
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.auditLogsPage.filters.take')}
          <select className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm" value={auditTake} onChange={(event) => setAuditTake(Number(event.target.value))}>
            <option value={20}>20</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </label>
      </LogsFilterBar>

      {!auditUserId ? (
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center text-slate-500">
          Please search and select a user to view their audit logs.
        </div>
      ) : null}

      {auditUserId ? (
      <section className="rounded-xl border border-slate-800 bg-slate-900 p-4 font-mono text-sm">
        <div className="mb-4 text-green-500/80">$ tail -f /var/log/authz/audit.log</div>
        {!auditLogs.length ? (
          <div className="text-slate-500 italic">{t('admin.auditLogsPage.messages.empty')}</div>
        ) : (
          <div className="space-y-1">
            {auditLogs.map((item) => {
              const isError = !item.isSuccess
              return (
                <div key={item.auditLogId} className="flex gap-3 rounded px-2 py-1 hover:bg-slate-800/60">
                  <span className="shrink-0 text-slate-500">[{new Date(item.occurredAtUtc).toISOString()}]</span>
                  <span className="w-28 shrink-0 text-blue-400">{item.action}</span>
                  <span className="w-24 shrink-0 text-amber-300">{item.targetType}</span>
                  <span className="min-w-0 flex-1 truncate text-slate-300" title={item.targetKey ?? ''}>{item.targetKey ?? '-'}</span>
                  <span className={`w-20 shrink-0 text-right font-semibold ${isError ? 'text-rose-400' : 'text-emerald-400'}`}>
                    {item.isSuccess ? 'OK' : 'FAIL'}
                  </span>
                  <button
                    type="button"
                    className="w-14 shrink-0 rounded border border-slate-700 px-1 text-xs text-slate-300 hover:bg-slate-700"
                    onClick={() =>
                      void copyLine(
                        `[${new Date(item.occurredAtUtc).toISOString()}] ${item.action} ${item.targetType} ${item.targetKey ?? '-'} ${item.isSuccess ? 'OK' : 'FAIL'}`,
                      )
                    }
                  >
                    Copy
                  </button>
                </div>
              )
            })}
          </div>
        )}
      </section>
      ) : null}
    </div>
  )
}
