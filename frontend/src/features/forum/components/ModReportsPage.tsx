import { useModReportsPage } from '../hooks/useModReportsPage'

function formatTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`
}

const FEEDBACK_STYLES = {
  success: 'border-slate-200 bg-slate-50 text-slate-800',
  error: 'border-rose-200 bg-rose-50 text-rose-800',
  forbidden: 'border-slate-200 bg-slate-50 text-slate-800',
}

export function ModReportsPage() {
  const {
    t,
    status,
    setStatus,
    tabs,
    reports,
    totalCount,
    totalPages,
    page,
    hasPreviousPage,
    hasNextPage,
    isLoading,
    isError,
    isFetching,
    isResolving,
    feedback,
    onResolve,
    onPageChange,
    getReasonLabel,
    getItemLink,
  } = useModReportsPage()

  if (isLoading) {
    return <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600">{t('common.loading')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
        {t('forum.mod.feedback.loadFailed')}
      </div>
    )
  }

  return (
    <section className="space-y-3">
      <header className="rounded-lg border border-slate-200 bg-white p-4">
        <h2 className="text-base font-semibold text-slate-900">{t('forum.mod.reportsTitle')}</h2>
        <p className="mt-1 text-sm text-slate-600">
          {t('forum.mod.totalReports')}: <span className="font-semibold">{totalCount}</span>
          {isFetching && <span className="ml-2 text-xs text-slate-400">{t('common.loading')}</span>}
        </p>
      </header>

      {/* Status filter tabs */}
      <div className="flex flex-wrap gap-2">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setStatus(tab.id)}
            className={`rounded-md border px-3 py-1.5 text-xs font-medium ${
              status === tab.id
                ? 'border-slate-800 bg-slate-900 text-white'
                : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Feedback toast */}
      {feedback ? (
        <div className={`rounded-md border px-3 py-2 text-sm ${FEEDBACK_STYLES[feedback.type]}`}>
          {feedback.message}
        </div>
      ) : null}

      {/* Reports table */}
      <div className="overflow-hidden rounded-lg border border-slate-200 bg-white">
        <table className="w-full table-auto text-left text-sm">
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="px-3 py-2">{t('forum.mod.columns.type')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.reason')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.preview')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.createdAt')}</th>
              <th className="px-3 py-2">{t('forum.mod.columns.actions')}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {reports.map((report) => {
              const itemLink = getItemLink(report)
              const isPost = report.reportedItemType === 1

              return (
                <tr key={report.id}>
                  <td className="px-3 py-2">
                    <span
                      className={`rounded px-1.5 py-0.5 text-xs font-medium ${
                        isPost ? 'bg-slate-100 text-slate-800' : 'bg-slate-100 text-slate-700'
                      }`}
                    >
                      {isPost ? t('forum.mod.itemType.post') : t('forum.mod.itemType.comment')}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-slate-700">{getReasonLabel(report.reason)}</td>
                  <td className="max-w-xs px-3 py-2 text-slate-600">
                    {report.isTargetDeleted ? (
                      <span className="italic text-slate-400">{t('forum.mod.targetDeleted')}</span>
                    ) : (
                      <span className="line-clamp-2">
                        {report.titlePreview ?? report.contentPreview ?? t('common.noData')}
                      </span>
                    )}
                  </td>
                  <td className="px-3 py-2 text-slate-500">{formatTime(report.createdAt)}</td>
                  <td className="px-3 py-2">
                    <div className="flex flex-wrap gap-1.5">
                      {/* Open post/comment link */}
                      {itemLink && !report.isTargetDeleted ? (
                        <a
                          href={itemLink}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="rounded border border-slate-200 px-2 py-1 text-xs font-medium text-slate-600 hover:border-slate-400 hover:text-slate-900"
                        >
                          {isPost ? t('forum.mod.actions.openPost') : t('forum.mod.actions.openComment')}
                        </a>
                      ) : null}

                      {/* Resolve actions — only for pending queue */}
                      {status === 'pending' ? (
                        <>
                          <button
                            type="button"
                            disabled={isResolving}
                            onClick={() => void onResolve(report.id, 'keep')}
                            className="rounded border border-slate-200 px-2 py-1 text-xs font-medium text-slate-700 hover:border-slate-400 hover:bg-slate-50 disabled:opacity-60"
                          >
                            {t('forum.mod.actions.keep')}
                          </button>
                          <button
                            type="button"
                            disabled={isResolving}
                            onClick={() => void onResolve(report.id, 'remove')}
                            className="rounded border border-rose-200 px-2 py-1 text-xs font-medium text-rose-700 hover:bg-rose-50 disabled:opacity-60"
                          >
                            {t('forum.mod.actions.remove')}
                          </button>
                        </>
                      ) : (
                        <span className="text-xs text-slate-400">
                          {report.resolutionDecision === 'keep'
                            ? t('forum.mod.status.resolvedKeep')
                            : report.resolutionDecision === 'remove'
                              ? t('forum.mod.status.resolvedRemove')
                              : '-'}
                        </span>
                      )}
                    </div>
                  </td>
                </tr>
              )
            })}
            {reports.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-3 py-6 text-center text-sm text-slate-500">
                  {t('forum.mod.noReports')}
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 ? (
        <div className="flex items-center justify-between rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm">
          <span className="text-slate-500">
            {t('forum.mod.pagination.page', { page, total: totalPages })}
          </span>
          <div className="flex gap-2">
            <button
              type="button"
              disabled={!hasPreviousPage}
              onClick={() => onPageChange(page - 1)}
              className="rounded border border-slate-200 px-3 py-1 text-xs font-medium text-slate-700 hover:border-slate-400 hover:bg-slate-50 disabled:opacity-40"
            >
              {t('forum.mod.pagination.prev')}
            </button>
            <button
              type="button"
              disabled={!hasNextPage}
              onClick={() => onPageChange(page + 1)}
              className="rounded border border-slate-200 px-3 py-1 text-xs font-medium text-slate-700 hover:border-slate-400 hover:bg-slate-50 disabled:opacity-40"
            >
              {t('forum.mod.pagination.next')}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
