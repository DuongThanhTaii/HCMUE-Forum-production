import { useCallback, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ClipboardList, GitBranch, Layers, ListTree } from 'lucide-react'
import type { UserActionLogItemDto, UserActionLogsViewType } from '../../types/admin.types'
import { LogsFilterBar } from './LogsFilterBar'
import { useAdminLogsPage } from '../hooks/useAdminLogsPage'
import { useGetUsersQuery } from '../../api/admin.api'
import { ACTION_LOG_FEATURE_CLUSTERS, inferActionLogFeatureId } from '../lib/action-log-features'
import { ActionLogDetailDrawer } from './ActionLogDetailDrawer'
import type { ActionLogOutcomeFilter } from '../hooks/useAdminLogsPage'

export function AdminActionLogsPage() {
  const { t } = useTranslation()
  const {
    actionItems,
    actionPage,
    setActionPage,
    actionPageSize,
    setActionPageSize,
    actionTotal,
    actionViewType,
    setActionViewType,
    availableActionViewTypes,
    isActionLogsLoading,
    isActionLogsError,
    actionActorUserId,
    setActionActorUserId,
    actionFeatureId,
    setActionFeatureId,
    actionOutcome,
    setActionOutcome,
    actionCorrelationId,
    setActionCorrelationId,
    actionTraceId,
    setActionTraceId,
    actionQueryEnabled,
  } = useAdminLogsPage()

  const { data: usersData } = useGetUsersQuery()
  const users = usersData ?? []

  const [detailItem, setDetailItem] = useState<UserActionLogItemDto | null>(null)

  const pageCount = Math.max(1, Math.ceil(actionTotal / actionPageSize))

  const correlationMode = Boolean(actionCorrelationId.trim())
  const traceMode = Boolean(actionTraceId.trim() && !correlationMode)

  const featureLabel = useCallback(
    (id: string) => t(`admin.actionLogsPage.features.${id}`, { defaultValue: id }),
    [t],
  )

  const displayItems = useMemo(() => {
    if (correlationMode) {
      return [...actionItems].sort(
        (a, b) => new Date(a.startedAtUtc).getTime() - new Date(b.startedAtUtc).getTime(),
      )
    }
    return actionItems
  }, [actionItems, correlationMode])

  const stats = useMemo(() => {
    const errors = displayItems.filter((i) => i.statusCode >= 400).length
    const ok = displayItems.length - errors
    const ms = displayItems.map((i) => i.durationMs)
    const p95 =
      ms.length === 0
        ? 0
        : [...ms].sort((a, b) => a - b)[Math.floor(ms.length * 0.95)] ?? ms[ms.length - 1]
    return { errors, ok, p95, total: displayItems.length }
  }, [displayItems])

  const exportJson = () => {
    const payload = JSON.stringify(displayItems, null, 2)
    const blob = new Blob([payload], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const scope = actionCorrelationId.trim() || actionActorUserId.trim() || actionTraceId.trim() || 'export'
    a.download = `action-logs-${scope}-${new Date().toISOString()}.json`
    a.click()
    URL.revokeObjectURL(url)
  }

  const copyText = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
    } catch {
      /* ignore */
    }
  }

  const followCorrelation = (correlationId: string) => {
    setActionCorrelationId(correlationId)
    setActionPageSize((s) => (s < 200 ? 200 : s))
  }

  const onCorrelationInput = (value: string) => {
    setActionCorrelationId(value)
    if (value.trim()) {
      setActionPageSize((s) => (s < 200 ? 200 : s))
    }
  }

  const outcomeOptions: { id: ActionLogOutcomeFilter; label: string }[] = [
    { id: 'all', label: t('admin.actionLogsPage.filters.outcomeAll') },
    { id: 'success', label: t('admin.actionLogsPage.filters.outcomeSuccess') },
    { id: 'client_error', label: t('admin.actionLogsPage.filters.outcomeClientError') },
    { id: 'server_error', label: t('admin.actionLogsPage.filters.outcomeServerError') },
  ]

  return (
    <div className="mx-auto max-w-[1600px] space-y-5 pb-10">
      <header className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <div className="flex items-center gap-2 text-slate-600">
              <ClipboardList className="h-5 w-5" aria-hidden />
              <span className="text-[11px] font-semibold uppercase tracking-widest">
                {t('admin.actionLogsPage.hero.kicker')}
              </span>
            </div>
            <h1 className="mt-2 text-xl font-semibold tracking-tight text-slate-900 md:text-2xl">
              {t('admin.actionLogsPage.title')}
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-relaxed text-slate-600">
              {t('admin.actionLogsPage.hero.subtitle')}
            </p>
          </div>
          <div className="flex flex-col items-end gap-2">
            <span className="rounded border border-slate-200 bg-slate-50 px-2.5 py-1 font-mono text-xs text-slate-700">
              {t('admin.actionLogsPage.hero.autoRefresh')}
            </span>
            <button
              type="button"
              onClick={exportJson}
              disabled={!displayItems.length}
              className="cursor-pointer rounded-md border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-800 shadow-sm transition-colors hover:bg-slate-50 disabled:opacity-50"
            >
              {t('admin.actionLogsPage.hero.export')}
            </button>
          </div>
        </div>

        {actionQueryEnabled && !isActionLogsLoading && !isActionLogsError ? (
          <div className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <div className="rounded-lg border border-slate-200 bg-slate-50/50 p-4">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.actionLogsPage.stats.inPage')}
              </p>
              <p className="mt-1 text-xl font-semibold tabular-nums text-slate-900">{stats.total}</p>
              <p className="text-xs text-slate-500">{t('admin.actionLogsPage.stats.inPageHint')}</p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.actionLogsPage.stats.ok')}
              </p>
              <p className="mt-1 text-xl font-semibold tabular-nums text-slate-900">{stats.ok}</p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.actionLogsPage.stats.errors')}
              </p>
              <p className="mt-1 text-xl font-semibold tabular-nums text-slate-900">{stats.errors}</p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.actionLogsPage.stats.p95')}
              </p>
              <p className="mt-1 text-xl font-semibold tabular-nums text-slate-900">{stats.p95} ms</p>
            </div>
          </div>
        ) : null}
      </header>

      <div className="inline-flex flex-wrap gap-0.5 rounded-lg border border-slate-200 bg-slate-50 p-1">
        {availableActionViewTypes.map((vt) => (
          <button
            key={vt}
            type="button"
            className={`flex cursor-pointer flex-col items-stretch rounded-md px-3 py-1.5 text-left transition-colors ${
              actionViewType === vt ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-600 hover:text-slate-900'
            }`}
            onClick={() => setActionViewType(vt as UserActionLogsViewType)}
          >
            <span className="text-sm font-medium">{t(`admin.actionLogsPage.viewType.${vt}`)}</span>
            <span className="text-[10px] font-normal leading-tight text-slate-500">
              {t(`admin.actionLogsPage.viewType.${vt}Hint`)}
            </span>
          </button>
        ))}
      </div>

      {correlationMode ? (
        <div className="flex flex-wrap items-center gap-3 rounded-lg border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-800">
          <GitBranch className="h-5 w-5 shrink-0 text-slate-500" aria-hidden />
          <div className="min-w-0 flex-1">
            <p className="font-semibold text-slate-900">{t('admin.actionLogsPage.banner.correlationTitle')}</p>
            <p className="mt-0.5 break-all font-mono text-xs text-slate-700">{actionCorrelationId.trim()}</p>
            <p className="mt-1 text-xs text-slate-600">{t('admin.actionLogsPage.banner.correlationBody')}</p>
          </div>
          <ListTree className="h-6 w-6 shrink-0 text-slate-400" aria-hidden />
        </div>
      ) : traceMode ? (
        <div className="flex flex-wrap items-center gap-3 rounded-lg border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-800">
          <Layers className="h-5 w-5 shrink-0 text-slate-500" aria-hidden />
          <div>
            <p className="font-semibold text-slate-900">{t('admin.actionLogsPage.banner.traceTitle')}</p>
            <p className="mt-1 text-xs text-slate-600">{t('admin.actionLogsPage.banner.traceBody')}</p>
          </div>
        </div>
      ) : null}

      <LogsFilterBar className="grid grid-cols-1 gap-4 rounded-xl border border-slate-200 bg-white p-4 xl:grid-cols-6">
        <label className="text-sm font-semibold text-slate-800 xl:col-span-2">
          {t('admin.actionLogsPage.filters.user')}
          <input
            list="users-list"
            className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm shadow-sm"
            placeholder={t('admin.actionLogsPage.filters.userPlaceholder')}
            value={actionActorUserId}
            onChange={(e) => setActionActorUserId(e.target.value)}
          />
          <datalist id="users-list">
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName} ({u.email})
              </option>
            ))}
          </datalist>
        </label>

        <label className="text-sm font-semibold text-slate-800 xl:col-span-2">
          {t('admin.actionLogsPage.filters.feature')}
          <select
            className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm shadow-sm"
            value={actionFeatureId}
            onChange={(e) => setActionFeatureId(e.target.value)}
          >
            {ACTION_LOG_FEATURE_CLUSTERS.map((c) => (
              <option key={c.id} value={c.id}>
                {featureLabel(c.id)}
              </option>
            ))}
          </select>
        </label>

        <label className="text-sm font-semibold text-slate-800 xl:col-span-2">
          {t('admin.actionLogsPage.filters.correlationId')}
          <input
            className="mt-1.5 w-full rounded-lg border border-violet-300 bg-violet-50/30 px-3 py-2.5 font-mono text-sm shadow-sm"
            placeholder={t('admin.actionLogsPage.filters.correlationPlaceholder')}
            value={actionCorrelationId}
            onChange={(e) => onCorrelationInput(e.target.value)}
          />
          <span className="mt-1 block text-xs font-normal text-slate-500">
            {t('admin.actionLogsPage.filters.correlationHint')}
          </span>
        </label>

        <label className="text-sm font-semibold text-slate-800 xl:col-span-2">
          {t('admin.actionLogsPage.filters.traceId')}
          <input
            className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2.5 font-mono text-sm shadow-sm"
            placeholder={t('admin.actionLogsPage.filters.tracePlaceholder')}
            value={actionTraceId}
            onChange={(e) => setActionTraceId(e.target.value)}
          />
        </label>

        <div className="xl:col-span-4">
          <p className="text-sm font-semibold text-slate-800">{t('admin.actionLogsPage.filters.outcome')}</p>
          <div className="mt-1.5 flex flex-wrap gap-2">
            {outcomeOptions.map((o) => (
              <button
                key={o.id}
                type="button"
                onClick={() => setActionOutcome(o.id)}
                className={`cursor-pointer rounded-full border px-3 py-1.5 text-xs font-medium transition-colors ${
                  actionOutcome === o.id
                    ? 'border-slate-800 bg-slate-800 text-white'
                    : 'border-slate-300 bg-white text-slate-700 hover:border-slate-400'
                }`}
              >
                {o.label}
              </button>
            ))}
          </div>
        </div>

        <label className="text-sm font-semibold text-slate-800">
          {t('admin.actionLogsPage.filters.pageSize')}
          <select
            className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm shadow-sm"
            value={actionPageSize}
            onChange={(e) => setActionPageSize(Number(e.target.value))}
          >
            <option value={50}>50</option>
            <option value={100}>100</option>
            <option value={200}>200</option>
            <option value={300}>300</option>
          </select>
        </label>
      </LogsFilterBar>

      {!actionQueryEnabled ? (
        <div className="rounded-lg border border-dashed border-slate-300 bg-white p-14 text-center">
          <ClipboardList className="mx-auto h-10 w-10 text-slate-400" aria-hidden />
          <p className="mt-4 text-base font-semibold text-slate-800">{t('admin.actionLogsPage.empty.needScopeTitle')}</p>
          <p className="mx-auto mt-2 max-w-lg text-sm text-slate-600">{t('admin.actionLogsPage.empty.needScopeBody')}</p>
        </div>
      ) : isActionLogsLoading ? (
        <div className="flex min-h-[520px] items-center justify-center rounded-2xl border border-slate-800 bg-slate-950 text-lg text-emerald-400">
          {t('admin.actionLogsPage.loading')}
        </div>
      ) : isActionLogsError ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-rose-800">
          {t('admin.actionLogsPage.messages.loadError')}
        </div>
      ) : correlationMode ? (
        <section className="relative min-h-[560px] rounded-2xl border border-slate-800 bg-slate-950 p-6 shadow-inner">
          <div className="mb-6 font-mono text-sm text-emerald-500/80">
            $ {t('admin.actionLogsPage.timeline.prompt', { id: actionCorrelationId.trim() })}
          </div>
          {displayItems.length === 0 ? (
            <p className="text-slate-500">{t('admin.actionLogsPage.messages.empty')}</p>
          ) : (
            <ul className="relative space-y-0 pl-2 before:absolute before:left-[15px] before:top-2 before:h-[calc(100%-16px)] before:w-px before:bg-emerald-700/50">
              {displayItems.map((item, idx) => {
                const isError = item.statusCode >= 400
                const feat = inferActionLogFeatureId(item.path)
                return (
                  <li key={item.id} className="relative flex gap-4 pb-8 pl-8 last:pb-0">
                    <span className="absolute left-0 top-1 flex h-8 w-8 items-center justify-center rounded-full border-2 border-slate-800 bg-slate-900 font-mono text-xs font-bold text-emerald-400">
                      {idx + 1}
                    </span>
                    <button
                      type="button"
                      onClick={() => setDetailItem(item)}
                      className="min-w-0 flex-1 rounded-xl border border-slate-700 bg-slate-900/80 p-4 text-left shadow-lg transition hover:border-rose-500/60 hover:bg-slate-900"
                    >
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="font-mono text-xs text-slate-500">
                          {new Date(item.startedAtUtc).toISOString()}
                        </span>
                        <span className="rounded bg-blue-950 px-2 py-0.5 font-mono text-sm font-bold text-blue-300">
                          {item.method}
                        </span>
                        <span
                          className={`rounded px-2 py-0.5 font-mono text-sm font-bold ${
                            isError ? 'bg-rose-950 text-rose-300' : 'bg-emerald-950 text-emerald-300'
                          }`}
                        >
                          {item.statusCode}
                        </span>
                        <span className="rounded-full bg-slate-800 px-2 py-0.5 text-xs font-semibold text-amber-200">
                          {featureLabel(feat)}
                        </span>
                        <span className="text-xs text-amber-300/90">{item.durationMs} ms</span>
                      </div>
                      <p className="mt-2 break-all font-mono text-sm text-slate-200">{item.path}</p>
                      <p className="mt-1 truncate font-mono text-xs text-slate-500" title={item.endpoint}>
                        {item.endpoint}
                      </p>
                    </button>
                  </li>
                )
              })}
            </ul>
          )}
        </section>
      ) : (
        <section className="min-h-[520px] overflow-hidden rounded-2xl border border-slate-800 bg-slate-950 shadow-inner">
          <div className="sticky top-0 z-10 border-b border-slate-800 bg-slate-900/95 px-4 py-3 font-mono text-xs text-emerald-500">
            $ tail -f /var/log/users/{actionActorUserId || actionTraceId || '…'}.log
          </div>
          <div className="max-h-[640px] overflow-y-auto p-4 font-mono text-sm">
            {displayItems.length === 0 ? (
              <div className="text-slate-500">{t('admin.actionLogsPage.messages.empty')}</div>
            ) : (
              <div className="space-y-1">
                {displayItems.map((item) => {
                  const isError = item.statusCode >= 400
                  const feat = inferActionLogFeatureId(item.path)
                  return (
                    <div
                      key={item.id}
                      className="flex flex-wrap items-center gap-2 rounded-lg px-2 py-2 hover:bg-slate-800/60"
                    >
                      <span className="text-slate-500 shrink-0 text-[11px]">
                        [{new Date(item.startedAtUtc).toISOString()}]
                      </span>
                      <span className="w-12 shrink-0 font-bold text-blue-400">{item.method}</span>
                      <span className="hidden shrink-0 rounded-full bg-slate-800 px-2 py-0.5 text-[10px] font-semibold text-amber-200 md:inline">
                        {featureLabel(feat)}
                      </span>
                      <button
                        type="button"
                        className="min-w-0 flex-1 truncate text-left text-slate-300 hover:text-white"
                        title={item.path}
                        onClick={() => setDetailItem(item)}
                      >
                        {item.path}
                      </button>
                      <span className={`w-10 shrink-0 font-bold ${isError ? 'text-rose-400' : 'text-emerald-400'}`}>
                        {item.statusCode}
                      </span>
                      <span className="w-14 shrink-0 text-right text-amber-300">{item.durationMs}ms</span>
                      <button
                        type="button"
                        className="shrink-0 rounded border border-slate-600 px-2 py-0.5 text-[10px] text-slate-300 hover:bg-slate-700"
                        onClick={() => void copyText(item.correlationId)}
                        title={t('admin.actionLogsPage.row.copyCorrelation')}
                      >
                        CID
                      </button>
                      <button
                        type="button"
                        className="shrink-0 rounded border border-slate-600 px-2 py-0.5 text-[10px] text-slate-300 hover:bg-slate-700"
                        onClick={() =>
                          void copyText(
                            `[${new Date(item.startedAtUtc).toISOString()}] ${item.method} ${item.path} ${item.statusCode} ${item.durationMs}ms`,
                          )
                        }
                      >
                        {t('admin.actionLogsPage.row.copy')}
                      </button>
                    </div>
                  )
                })}
              </div>
            )}
          </div>
        </section>
      )}

      {actionQueryEnabled && !isActionLogsLoading && !isActionLogsError ? (
        <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm text-slate-600">
            {t('admin.actionLogsPage.pagination.summary', {
              total: actionTotal,
              from: (actionPage - 1) * actionPageSize + (displayItems.length ? 1 : 0),
              to: Math.min(actionPage * actionPageSize, actionTotal),
            })}
          </p>
          <div className="flex items-center gap-2">
            <button
              type="button"
              className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
              onClick={() => setActionPage(Math.max(1, actionPage - 1))}
              disabled={actionPage <= 1}
            >
              {t('admin.actionLogsPage.pagination.previous')}
            </button>
            <p className="text-sm font-semibold text-slate-800">
              {t('admin.actionLogsPage.pagination.page')} {actionPage} / {pageCount}
            </p>
            <button
              type="button"
              className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:opacity-50"
              onClick={() => setActionPage(Math.min(pageCount, actionPage + 1))}
              disabled={actionPage >= pageCount}
            >
              {t('admin.actionLogsPage.pagination.next')}
            </button>
          </div>
        </div>
      ) : null}

      {detailItem ? (
        <ActionLogDetailDrawer
          item={detailItem}
          viewType={actionViewType}
          featureLabel={featureLabel}
          onClose={() => setDetailItem(null)}
          onFollowCorrelation={followCorrelation}
        />
      ) : null}
    </div>
  )
}
