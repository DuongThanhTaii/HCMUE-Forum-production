import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useGetModerationPendingPostsQuery, useGetModerationReportsQuery } from '../api/forum.moderation.api'
import { useGetDocumentsQuery } from '@features/learning/api/learning.api'

const metricValueClass = 'mt-1 text-2xl font-semibold tabular-nums text-slate-900'

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

export function ModDashboardPage() {
  const { t } = useTranslation()
  const { data: pendingReports } = useGetModerationReportsQuery({ status: 'pending', pageNumber: 1, pageSize: 1 })
  const { data: keepReports } = useGetModerationReportsQuery({ status: 'resolved_keep', pageNumber: 1, pageSize: 1 })
  const { data: removeReports } = useGetModerationReportsQuery({ status: 'resolved_remove', pageNumber: 1, pageSize: 1 })
  const { data: pendingPosts = [] } = useGetModerationPendingPostsQuery({ pageNumber: 1, pageSize: 50 })
  const { data: pendingDocuments } = useGetDocumentsQuery({ pageNumber: 1, pageSize: 1, status: 2 })

  const pending = pendingReports?.totalCount ?? 0
  const kept = keepReports?.totalCount ?? 0
  const removed = removeReports?.totalCount ?? 0
  const docsPending = pendingDocuments?.totalCount ?? 0
  const totalResolved = kept + removed
  const totalAll = pending + totalResolved
  const pendingRate = totalAll > 0 ? (pending / totalAll) * 100 : 0
  const removedRate = totalResolved > 0 ? (removed / totalResolved) * 100 : 0

  const weeklyPending = useMemo(
    () => bucketLastNDays(
      pendingPosts.map((p) => p.createdAt),
      7,
    ),
    [pendingPosts],
  )
  const maxWeekly = Math.max(1, ...weeklyPending)

  const dayLabels = useMemo(() => {
    const fmt = new Intl.DateTimeFormat(undefined, { weekday: 'short' })
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date()
      d.setDate(d.getDate() - (6 - i))
      return fmt.format(d)
    })
  }, [])

  const leaderboard = useMemo(() => {
    const m = new Map<string, number>()
    for (const p of pendingPosts) {
      const name = (p.authorName ?? '').trim() || t('mod.dashboard.charts.leaderboardUnknown')
      m.set(name, (m.get(name) ?? 0) + 1)
    }
    return [...m.entries()].sort((a, b) => b[1] - a[1]).slice(0, 5)
  }, [pendingPosts, t])

  const forumLoad = pending + pendingPosts.length
  const learningLoad = docsPending
  const sumFL = Math.max(1, forumLoad + learningLoad)
  const forumPct = (forumLoad / sumFL) * 100
  const learningPct = (learningLoad / sumFL) * 100

  return (
    <section className="space-y-4">
      <header className="rounded-xl border border-slate-200 bg-gradient-to-br from-white via-slate-50/80 to-sky-50/40 p-5 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-900">{t('mod.dashboard.title')}</h2>
        <p className="mt-1 text-sm text-slate-600">{t('mod.dashboard.subtitle')}</p>
      </header>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{t('mod.dashboard.metrics.pendingReports')}</p>
          <p className={metricValueClass}>{pending}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{t('mod.dashboard.metrics.pendingPosts')}</p>
          <p className={metricValueClass}>{pendingPosts.length}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{t('mod.dashboard.metrics.pendingDocuments')}</p>
          <p className={metricValueClass}>{docsPending}</p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{t('mod.dashboard.metrics.resolved')}</p>
          <p className={metricValueClass}>{totalResolved}</p>
        </div>
      </div>

      <div className="grid gap-3 xl:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('mod.dashboard.charts.forumVsLearningTitle')}</p>
          <div className="mt-3 h-3 w-full overflow-hidden rounded-full bg-slate-100">
            <div className="flex h-full w-full">
              <div
                className="bg-gradient-to-r from-indigo-600 to-indigo-400 transition-all"
                style={{ width: `${forumPct}%` }}
                title={t('mod.dashboard.charts.forumVsLearningForum')}
              />
              <div
                className="bg-gradient-to-r from-emerald-600 to-teal-400 transition-all"
                style={{ width: `${learningPct}%` }}
                title={t('mod.dashboard.charts.forumVsLearningLearning')}
              />
            </div>
          </div>
          <div className="mt-3 flex flex-wrap gap-4 text-xs text-slate-600">
            <span className="inline-flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full bg-indigo-500" />
              {t('mod.dashboard.charts.forumVsLearningForum')}: <strong className="text-slate-900">{forumLoad}</strong>
            </span>
            <span className="inline-flex items-center gap-1.5">
              <span className="h-2 w-2 rounded-full bg-emerald-500" />
              {t('mod.dashboard.charts.forumVsLearningLearning')}: <strong className="text-slate-900">{learningLoad}</strong>
            </span>
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('mod.dashboard.charts.backlogTitle')}</p>
          <div className="mt-3 h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
            <div className="h-full bg-slate-700 transition-all" style={{ width: `${pendingRate}%` }} />
          </div>
          <p className="mt-2 text-xs text-slate-600">{t('mod.dashboard.charts.backlogHint', { pct: pendingRate.toFixed(1) })}</p>
        </div>
      </div>

      <div className="grid gap-3 xl:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('mod.dashboard.charts.removeRateTitle')}</p>
          <div className="mt-3 h-2.5 w-full overflow-hidden rounded-full bg-slate-100">
            <div className="h-full bg-slate-500 transition-all" style={{ width: `${removedRate}%` }} />
          </div>
          <p className="mt-2 text-xs text-slate-600">{t('mod.dashboard.charts.removeRateHint', { pct: removedRate.toFixed(1) })}</p>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
          <p className="text-sm font-semibold text-slate-800">{t('mod.dashboard.charts.weeklyPendingPostsTitle')}</p>
          <p className="mt-1 text-xs text-slate-500">{t('mod.dashboard.charts.weeklyPendingPostsHint')}</p>
          <div className="mt-4 flex h-36 items-end gap-1.5">
            {weeklyPending.map((c, i) => (
              <div key={i} className="flex flex-1 flex-col items-center gap-2">
                <div className="flex h-28 w-full items-end justify-center rounded-t-md bg-slate-50">
                  <div
                    className="w-[85%] rounded-t-md bg-gradient-to-t from-sky-700 to-sky-400 transition-all"
                    style={{ height: `${(c / maxWeekly) * 100}%`, minHeight: c ? 6 : 0 }}
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
        <p className="text-sm font-semibold text-slate-800">{t('mod.dashboard.charts.leaderboardTitle')}</p>
        <p className="mt-1 text-xs text-slate-500">{t('mod.dashboard.charts.leaderboardSubtitle')}</p>
        {leaderboard.length === 0 ? (
          <p className="mt-4 text-sm text-slate-500">{t('mod.dashboard.charts.leaderboardEmpty')}</p>
        ) : (
          <ol className="mt-4 space-y-2">
            {leaderboard.map(([name, count], idx) => (
              <li
                key={name}
                className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50/80 px-3 py-2 text-sm"
              >
                <span className="font-medium text-slate-700">
                  <span className="mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full bg-white text-xs font-bold text-indigo-700 shadow-sm">
                    {idx + 1}
                  </span>
                  {name}
                </span>
                <span className="tabular-nums text-slate-900">{count}</span>
              </li>
            ))}
          </ol>
        )}
      </div>
    </section>
  )
}
