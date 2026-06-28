import { useMemo, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { X, Copy } from 'lucide-react'
import type { UserActionLogItemDto, UserActionLogsViewType } from '../../types/admin.types'
import { inferActionLogFeatureId } from '../lib/action-log-features'

type ActionLogDetailDrawerProps = {
  item: UserActionLogItemDto | null
  /** Matches the action-log query view; Administrator responses omit HTTP payloads by API design. */
  viewType?: UserActionLogsViewType
  featureLabel: (id: string) => string
  onClose: () => void
  onFollowCorrelation: (correlationId: string) => void
}

function methodUsuallyHasNoRequestBody(method: string): boolean {
  const m = method.toUpperCase()
  return m === 'GET' || m === 'HEAD' || m === 'DELETE'
}

function formatPayloadText(body: string | null | undefined, contentType: string | null | undefined): string {
  if (body == null || body === '') return ''
  const ct = (contentType ?? '').toLowerCase()
  const looksJson =
    ct.includes('json') || /^\s*[\[{]/.test(body)
  if (!looksJson) return body
  try {
    return JSON.stringify(JSON.parse(body), null, 2)
  } catch {
    return body
  }
}

function formatHeadersJson(json: string | null | undefined): string {
  if (json == null || json === '') return ''
  try {
    return JSON.stringify(JSON.parse(json), null, 2)
  } catch {
    return json
  }
}

function MetadataRow({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-1 gap-1 border-b border-slate-100 py-2.5 last:border-b-0 sm:grid-cols-[140px_1fr] sm:gap-4">
      <dt className="text-xs font-medium text-slate-500">{label}</dt>
      <dd className="break-all text-sm text-slate-900">{children}</dd>
    </div>
  )
}

function Panel({
  title,
  subtitle,
  children,
  actions,
}: {
  title: string
  subtitle?: string | null
  children: ReactNode
  actions?: ReactNode
}) {
  return (
    <section className="overflow-hidden rounded-lg border border-slate-200 bg-white">
      <div className="flex flex-wrap items-start justify-between gap-2 border-b border-slate-200 bg-slate-50 px-4 py-2.5">
        <div>
          <h3 className="text-[11px] font-semibold uppercase tracking-widest text-slate-600">{title}</h3>
          {subtitle ? <p className="mt-0.5 text-xs text-slate-500">{subtitle}</p> : null}
        </div>
        {actions ? <div className="flex gap-1">{actions}</div> : null}
      </div>
      <div className="p-4">{children}</div>
    </section>
  )
}

export function ActionLogDetailDrawer({
  item,
  viewType = 'Developer',
  featureLabel,
  onClose,
  onFollowCorrelation,
}: ActionLogDetailDrawerProps) {
  const { t } = useTranslation()
  const isAdministratorListView = viewType === 'Administrator'

  const requestBodyDisplay = useMemo(
    () => formatPayloadText(item?.requestBodyPreview, item?.requestContentType),
    [item?.requestBodyPreview, item?.requestContentType],
  )
  const responseBodyDisplay = useMemo(
    () => formatPayloadText(item?.responseBodyPreview, item?.responseContentType),
    [item?.responseBodyPreview, item?.responseContentType],
  )
  const requestHeadersDisplay = useMemo(() => formatHeadersJson(item?.requestHeadersJson), [item?.requestHeadersJson])
  const responseHeadersDisplay = useMemo(() => formatHeadersJson(item?.responseHeadersJson), [item?.responseHeadersJson])

  if (!item) return null

  const copy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
    } catch {
      /* ignore */
    }
  }

  const fullUrl = `${item.scheme}://${item.host}${item.path}${item.queryString || ''}`
  const isError = item.statusCode >= 400
  const featId = inferActionLogFeatureId(item.path)

  const copyBtn = (text: string, label: string) => (
    <button
      type="button"
      onClick={() => void copy(text)}
      className="inline-flex cursor-pointer items-center gap-1 rounded border border-slate-300 bg-white px-2 py-1 text-xs font-medium text-slate-700 transition-colors hover:bg-slate-100"
      title={label}
    >
      <Copy className="h-3.5 w-3.5" />
      {t('admin.actionLogsPage.detail.copy')}
    </button>
  )

  return (
    <div
      className="fixed inset-0 z-50 flex justify-end bg-slate-900/40"
      role="dialog"
      aria-modal="true"
      aria-labelledby="action-log-detail-title"
      onClick={onClose}
    >
      <div
        className="flex h-full w-full max-w-3xl flex-col border-l border-slate-300 bg-white shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="flex shrink-0 items-start justify-between gap-4 border-b border-slate-200 bg-slate-50 px-5 py-4">
          <div className="min-w-0">
            <p id="action-log-detail-title" className="text-[11px] font-semibold uppercase tracking-widest text-slate-500">
              {t('admin.actionLogsPage.detail.kicker')}
            </p>
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <span className="rounded border border-slate-300 bg-white px-2 py-0.5 font-mono text-sm font-semibold text-slate-800">
                {item.method}
              </span>
              <span
                className={`rounded border px-2 py-0.5 font-mono text-sm font-semibold ${
                  isError ? 'border-rose-200 bg-rose-50 text-rose-800' : 'border-slate-200 bg-white text-slate-800'
                }`}
              >
                {item.statusCode}
              </span>
              <span className="rounded border border-slate-200 bg-white px-2 py-0.5 text-xs font-medium text-slate-600">
                {featureLabel(featId)}
              </span>
              <span className="text-xs tabular-nums text-slate-500">
                {item.durationMs} ms
              </span>
              <span
                className="rounded border border-slate-200 bg-slate-100 px-2 py-0.5 text-[10px] font-medium uppercase tracking-wide text-slate-600"
                title={t(`admin.actionLogsPage.viewType.${viewType}Hint`)}
              >
                {t('admin.actionLogsPage.detail.currentViewBadge')}: {t(`admin.actionLogsPage.viewType.${viewType}`)}
              </span>
            </div>
            <p className="mt-2 break-all font-mono text-sm leading-snug text-slate-800">{item.path}</p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="shrink-0 cursor-pointer rounded border border-slate-300 bg-white p-2 text-slate-600 transition-colors hover:bg-slate-100"
            aria-label={t('common.close')}
          >
            <X className="h-5 w-5" />
          </button>
        </header>

        <div className="flex-1 overflow-y-auto bg-slate-50/50 px-5 py-4">
          <div className="space-y-4">
            <Panel title={t('admin.actionLogsPage.detail.summary')} actions={copyBtn(fullUrl, '')}>
              <dl className="divide-y divide-slate-100">
                <MetadataRow label={t('admin.actionLogsPage.detail.endpoint')}>
                  <span className="font-mono text-xs">{item.endpoint}</span>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.fullUrl')}>
                  <span className="font-mono text-xs">{fullUrl}</span>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.correlation')}>
                  <div className="space-y-2">
                    <span className="font-mono text-xs">{item.correlationId || '—'}</span>
                    {item.correlationId ? (
                      <button
                        type="button"
                        onClick={() => {
                          onFollowCorrelation(item.correlationId)
                          onClose()
                        }}
                        className="cursor-pointer rounded border border-slate-800 bg-slate-800 px-3 py-1.5 text-xs font-semibold text-white transition-colors hover:bg-slate-900"
                      >
                        {t('admin.actionLogsPage.detail.followCorrelation')}
                      </button>
                    ) : null}
                  </div>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.trace')}>
                  <span className="font-mono text-xs">{item.traceId || '—'}</span>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.actor')}>
                  <span className="font-mono text-xs">{item.actorUserId}</span>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.network')}>
                  <div className="space-y-1 text-xs">
                    <p>
                      <span className="text-slate-500">IP</span>{' '}
                      <span className="font-mono text-slate-900">{item.remoteIp || '—'}</span>
                    </p>
                    <p className="break-all text-slate-700">{item.userAgent || '—'}</p>
                  </div>
                </MetadataRow>
                <MetadataRow label={t('admin.actionLogsPage.detail.timing')}>
                  <div className="space-y-1 font-mono text-xs text-slate-800">
                    <p>
                      {t('admin.actionLogsPage.detail.started')}: {new Date(item.startedAtUtc).toISOString()}
                    </p>
                    <p>
                      {t('admin.actionLogsPage.detail.completed')}: {new Date(item.completedAtUtc).toISOString()}
                    </p>
                  </div>
                </MetadataRow>
              </dl>
            </Panel>

            {isAdministratorListView ? (
              <Panel title={t('admin.actionLogsPage.detail.httpPayloadHiddenTitle')}>
                <p className="text-sm leading-relaxed text-slate-700">
                  {t('admin.actionLogsPage.detail.httpPayloadHiddenBody')}
                </p>
              </Panel>
            ) : (
              <>
                <Panel
                  title={t('admin.actionLogsPage.detail.requestSection')}
                  subtitle={item.requestContentType ?? undefined}
                  actions={
                    requestBodyDisplay ? copyBtn(item.requestBodyPreview ?? requestBodyDisplay, '') : undefined
                  }
                >
                  <div className="space-y-3">
                    <div>
                      <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                        {t('admin.actionLogsPage.detail.headers')}
                      </p>
                      <pre className="max-h-40 overflow-auto rounded border border-slate-200 bg-white p-3 font-mono text-xs leading-relaxed text-slate-800">
                        {requestHeadersDisplay || '—'}
                      </pre>
                    </div>
                    <div>
                      <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                        {t('admin.actionLogsPage.detail.body')}
                      </p>
                      <pre className="max-h-56 overflow-auto rounded border border-slate-200 bg-white p-3 font-mono text-xs leading-relaxed text-slate-800">
                        {requestBodyDisplay || '—'}
                      </pre>
                      {!requestBodyDisplay && methodUsuallyHasNoRequestBody(item.method) ? (
                        <p className="mt-1 text-xs text-slate-600">{t('admin.actionLogsPage.detail.noRequestBodyForMethod')}</p>
                      ) : null}
                      {item.requestBodyTruncated ? (
                        <p className="mt-1 text-xs text-amber-800">{t('admin.actionLogsPage.detail.bodyTruncated')}</p>
                      ) : null}
                    </div>
                  </div>
                </Panel>

                <Panel
                  title={t('admin.actionLogsPage.detail.responseSection')}
                  subtitle={item.responseContentType ?? undefined}
                  actions={
                    responseBodyDisplay ? copyBtn(item.responseBodyPreview ?? responseBodyDisplay, '') : undefined
                  }
                >
                  <div className="space-y-3">
                    <div>
                      <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                        {t('admin.actionLogsPage.detail.headers')}
                      </p>
                      <pre className="max-h-40 overflow-auto rounded border border-slate-200 bg-white p-3 font-mono text-xs leading-relaxed text-slate-800">
                        {responseHeadersDisplay || '—'}
                      </pre>
                    </div>
                    <div>
                      <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                        {t('admin.actionLogsPage.detail.body')}
                      </p>
                      <pre className="max-h-56 overflow-auto rounded border border-slate-200 bg-white p-3 font-mono text-xs leading-relaxed text-slate-800">
                        {responseBodyDisplay || '—'}
                      </pre>
                      {!responseBodyDisplay && item.statusCode === 204 ? (
                        <p className="mt-1 text-xs text-slate-600">{t('admin.actionLogsPage.detail.noResponseBody204')}</p>
                      ) : null}
                      {item.responseBodyTruncated ? (
                        <p className="mt-1 text-xs text-amber-800">{t('admin.actionLogsPage.detail.bodyTruncated')}</p>
                      ) : null}
                    </div>
                  </div>
                </Panel>
              </>
            )}

            {(item.exceptionType || item.exceptionMessage) && (
              <Panel title={t('admin.actionLogsPage.detail.exception')}>
                {item.exceptionType ? (
                  <p className="font-mono text-sm font-semibold text-rose-900">{item.exceptionType}</p>
                ) : null}
                {item.exceptionMessage ? (
                  <pre className="mt-2 whitespace-pre-wrap break-words font-mono text-sm text-rose-900">{item.exceptionMessage}</pre>
                ) : null}
              </Panel>
            )}

            <Panel
              title={t('admin.actionLogsPage.detail.terminalLine')}
              actions={copyBtn(item.terminalLine, '')}
            >
              <pre className="max-h-32 overflow-auto rounded border border-slate-200 bg-slate-900 p-3 font-mono text-xs text-slate-200">
                {item.terminalLine}
              </pre>
            </Panel>
          </div>
        </div>
      </div>
    </div>
  )
}
