import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useGetRolesQuery, useGetUsersQuery } from '../api/admin.api'
import { useGetAuditLogsQuery, useGetTogglesQuery, useGetUserActionLogsQuery } from '../api/admin.observability.api'

function bucketLastNDays(isoDates: string[], days: number): number[] {
  const buckets = Array.from({ length: days }, () => 0)
  const now = Date.now()
  for (const iso of isoDates) {
    const t = new Date(iso).getTime()
    const diff = Math.floor((now - t) / 86400000)
    if (diff >= 0 && diff < days) {
      buckets[days - 1 - diff]++
    }
  }
  return buckets
}

export function AdminDashboardPage() {
  const { t } = useTranslation()
  const { data: users = [] } = useGetUsersQuery()
  const { data: roles = [] } = useGetRolesQuery()
  const { data: toggles = [] } = useGetTogglesQuery()
  const { data: actions } = useGetUserActionLogsQuery({ page: 1, pageSize: 300, viewType: 'Administrator' })
  const { data: audits = [] } = useGetAuditLogsQuery({ take: 250 })

  const enabledToggles = toggles.filter((item) => item.isEnabled).length
  const actionItems = actions?.items ?? []
  const actionErrors = actionItems.filter((item) => item.statusCode >= 400).length
  const auditFailures = audits.filter((item) => !item.isSuccess).length
  const auditFailureRate = audits.length > 0 ? (auditFailures / audits.length) * 100 : 0
  const actionErrorRate = actionItems.length > 0 ? (actionErrors / actionItems.length) * 100 : 0

  const togglePct = toggles.length > 0 ? (enabledToggles / toggles.length) * 100 : 0

  const weeklyAudits = useMemo(
    () => bucketLastNDays(
      audits.map((a) => a.occurredAtUtc),
      7,
    ),
    [audits],
  )
  const weeklyActions = useMemo(
    () => bucketLastNDays(
      actionItems.map((a) => a.startedAtUtc),
      7,
    ),
    [actionItems],
  )
  const maxAudit = Math.max(1, ...weeklyAudits)
  const maxAction = Math.max(1, ...weeklyActions)

  const dayLabels = useMemo(() => {
    const fmt = new Intl.DateTimeFormat(undefined, { weekday: 'short' })
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date()
      d.setDate(d.getDate() - (6 - i))
      return fmt.format(d)
    })
  }, [])

  const topEndpoints = useMemo(() => {
    const m = new Map<string, number>()
    for (const item of actionItems) {
      const key = item.endpoint?.trim() || item.path?.trim() || '—'
      m.set(key, (m.get(key) ?? 0) + 1)
    }
    return [...m.entries()].sort((a, b) => b[1] - a[1]).slice(0, 5)
  }, [actionItems])

  return (
    <section className="space-y-5">
      <header className="rounded-xl border border-rose-100 bg-gradient-to-br from-white via-rose-50/40 to-indigo-50/30 p-5 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-900">{t('admin.dashboardPage.title')}</h2>
        <p className="mt-1 text-sm text-slate-600">{t('admin.dashboardPage.subtitle')}</p>
      </header>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs uppercase tracking-wide text-slate-500">{t('admin.dashboardPage.kpis.users')}</p>
          <p className="mt-1 text-3xl font-bold tabular-nums text-slate-900">{users.length}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs uppercase tracking-wide text-slate-500">{t('admin.dashboardPage.kpis.roles')}</p>
          <p className="mt-1 text-3xl font-bold tabular-nums text-indigo-700">{roles.length}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs uppercase tracking-wide text-slate-500">{t('admin.dashboardPage.kpis.endpointsOn')}</p>
          <p className="mt-1 text-3xl font-bold tabular-nums text-emerald-700">
            {enabledToggles} / {toggles.length}
          </p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs uppercase tracking-wide text-slate-500">{t('admin.dashboardPage.kpis.actionLogsSample')}</p>
          <p className="mt-1 text-3xl font-bold tabular-nums text-amber-700">{actionItems.length}</p>
        </div>
      </div>

      <div className="grid gap-3 xl:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('admin.dashboardPage.endpointHealth')}</p>
          <div className="mt-3 h-4 w-full overflow-hidden rounded-full bg-slate-100">
            <div
              className="h-full rounded-full bg-gradient-to-r from-emerald-600 to-teal-400 shadow-inner transition-all"
              style={{ width: `${togglePct}%` }}
            />
          </div>
          <p className="mt-2 text-xs text-slate-600">
            {t('admin.dashboardPage.endpointHealthHint', { enabled: enabledToggles, total: toggles.length || 0 })}
          </p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('admin.dashboardPage.errorRatesTitle')}</p>
          <div className="mt-3 space-y-3">
            <div>
              <div className="flex justify-between text-xs text-slate-600">
                <span>{t('admin.dashboardPage.actionErrorsLabel')}</span>
                <span className="tabular-nums">
                  {actionErrors}/{actionItems.length || 0} ({actionErrorRate.toFixed(1)}%)
                </span>
              </div>
              <div className="mt-1 h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
                <div className="h-full bg-amber-500 transition-all" style={{ width: `${actionErrorRate}%` }} />
              </div>
            </div>
            <div>
              <div className="flex justify-between text-xs text-slate-600">
                <span>{t('admin.dashboardPage.auditFailuresLabel')}</span>
                <span className="tabular-nums">
                  {auditFailures}/{audits.length || 0} ({auditFailureRate.toFixed(1)}%)
                </span>
              </div>
              <div className="mt-1 h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
                <div className="h-full bg-rose-500 transition-all" style={{ width: `${auditFailureRate}%` }} />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-3 xl:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('admin.dashboardPage.weeklyAuditTitle')}</p>
          <p className="mt-1 text-xs text-slate-500">{t('admin.dashboardPage.weeklyAuditHint')}</p>
          <div className="mt-4 flex h-36 items-end gap-1.5">
            {weeklyAudits.map((c, i) => (
              <div key={i} className="flex flex-1 flex-col items-center gap-2">
                <div className="flex h-28 w-full items-end justify-center rounded-t-md bg-slate-50">
                  <div
                    className="w-[85%] rounded-t-md bg-gradient-to-t from-violet-700 to-violet-400 transition-all"
                    style={{ height: `${(c / maxAudit) * 100}%`, minHeight: c ? 6 : 0 }}
                  />
                </div>
                <span className="text-center text-[10px] font-medium leading-tight text-slate-500">{dayLabels[i]}</span>
                <span className="text-[11px] tabular-nums text-slate-700">{c}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('admin.dashboardPage.weeklyActionsTitle')}</p>
          <p className="mt-1 text-xs text-slate-500">{t('admin.dashboardPage.weeklyActionsHint')}</p>
          <div className="mt-4 flex h-36 items-end gap-1.5">
            {weeklyActions.map((c, i) => (
              <div key={i} className="flex flex-1 flex-col items-center gap-2">
                <div className="flex h-28 w-full items-end justify-center rounded-t-md bg-slate-50">
                  <div
                    className="w-[85%] rounded-t-md bg-gradient-to-t from-amber-700 to-amber-400 transition-all"
                    style={{ height: `${(c / maxAction) * 100}%`, minHeight: c ? 6 : 0 }}
                  />
                </div>
                <span className="text-center text-[10px] font-medium leading-tight text-slate-500">{dayLabels[i]}</span>
                <span className="text-[11px] tabular-nums text-slate-700">{c}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <p className="text-sm font-semibold text-slate-800">{t('admin.dashboardPage.topEndpointsTitle')}</p>
        <p className="mt-1 text-xs text-slate-500">{t('admin.dashboardPage.topEndpointsHint', { count: topEndpoints.length })}</p>
        {topEndpoints.length === 0 ? (
          <p className="mt-4 text-sm text-slate-500">—</p>
        ) : (
          <ol className="mt-4 space-y-2">
            {topEndpoints.map(([path, count], idx) => (
              <li
                key={path}
                className="flex items-start justify-between gap-3 rounded-lg border border-slate-100 bg-slate-50/80 px-3 py-2 text-sm"
              >
                <span className="min-w-0 font-medium text-slate-800">
                  <span className="mr-2 inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-white text-xs font-bold text-rose-700 shadow-sm">
                    {idx + 1}
                  </span>
                  <span className="break-all font-mono text-[13px] text-slate-700">{path}</span>
                </span>
                <span className="shrink-0 tabular-nums text-slate-900">{count}</span>
              </li>
            ))}
          </ol>
        )}
      </div>
    </section>
  )
}
