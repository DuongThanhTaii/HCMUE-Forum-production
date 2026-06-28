import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ExternalLink, X } from 'lucide-react'
import { useGetForumPostByIdQuery } from '../api/forum.list.api'
import { useModerationHintMutation, type ModerationHintResult } from '@features/assistant/api/assistant.api'
import { featureFlags } from '@shared/config/featureFlags'
import { parseForumRichContent } from '../lib/parseForumRichContent'

type ModPostPreviewDrawerProps = {
  postId: string | null
  onClose: () => void
  onApprove: (postId: string) => void
  isPublishing: boolean
}

function readCopilotError(error: unknown, fallback: string): string {
  const raw = error as { data?: { error?: string }; error?: string; status?: number } | undefined
  return raw?.data?.error || raw?.error || fallback
}

function recommendationLabel(
  rec: string | undefined,
  t: (k: string) => string,
): string {
  const r = (rec ?? '').toLowerCase()
  if (r === 'allow') return t('forum.mod.pendingDetail.recAllow')
  if (r === 'review') return t('forum.mod.pendingDetail.recReview')
  if (r === 'hide') return t('forum.mod.pendingDetail.recHide')
  return rec ?? '—'
}

export function ModPostPreviewDrawer({ postId, onClose, onApprove, isPublishing }: ModPostPreviewDrawerProps) {
  const { t } = useTranslation()
  const [aiHint, setAiHint] = useState<ModerationHintResult | null>(null)
  const [aiError, setAiError] = useState<string | null>(null)

  const { data: post, isLoading, isError, isFetching } = useGetForumPostByIdQuery(postId ?? '', {
    skip: !postId,
  })

  const [runModerationHint, { isLoading: isAiLoading }] = useModerationHintMutation()

  useEffect(() => {
    setAiHint(null)
    setAiError(null)
  }, [postId])

  const parsedContent = useMemo(() => {
    if (!post) return { body: '', imageUrls: [] as string[], fileUrls: [] as string[] }
    const raw = (post.content ?? post.body ?? '').trim()
    return parseForumRichContent(raw)
  }, [post])

  if (!postId) return null

  const busy = isPublishing || isAiLoading
  const showAi = featureFlags.copilotActionsEnabled

  async function onAiHint() {
    if (!postId) return
    setAiError(null)
    try {
      const data = await runModerationHint({ postId }).unwrap()
      setAiHint(data)
    } catch (err) {
      setAiHint(null)
      setAiError(readCopilotError(err, t('forum.mod.pendingDetail.aiError')))
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex justify-end bg-slate-900/40"
      role="dialog"
      aria-modal="true"
      aria-labelledby="mod-pending-post-drawer-title"
      onClick={onClose}
    >
      <div
        className="flex h-full w-full max-w-xl flex-col border-l border-slate-300 bg-white shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="flex shrink-0 items-start justify-between gap-3 border-b border-slate-200 bg-slate-50 px-4 py-3">
          <div className="min-w-0">
            <p
              id="mod-pending-post-drawer-title"
              className="text-[11px] font-semibold uppercase tracking-widest text-slate-500"
            >
              {t('forum.mod.pendingDetail.drawerTitle')}
            </p>
            <p className="mt-1 line-clamp-2 text-sm font-semibold text-slate-900">
              {isLoading ? '…' : (post?.title ?? '—')}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="shrink-0 rounded border border-slate-300 bg-white p-2 text-slate-600 hover:bg-slate-100"
            aria-label={t('common.close')}
          >
            <X className="h-5 w-5" />
          </button>
        </header>

        <div className="flex-1 overflow-y-auto px-4 py-3">
          {isLoading || isFetching ? (
            <p className="text-sm text-slate-600">{t('common.loading')}</p>
          ) : isError || !post ? (
            <p className="text-sm text-rose-700">{t('forum.mod.pendingDetail.loadError')}</p>
          ) : (
            <div className="space-y-4">
              <div className="flex flex-wrap gap-2 text-xs text-slate-600">
                <span>
                  <span className="text-slate-500">{t('forum.mod.columns.author')}: </span>
                  {post.authorName ?? t('common.noData')}
                </span>
                <span className="text-slate-300">·</span>
                <span>
                  <span className="text-slate-500">{t('forum.mod.columns.category')}: </span>
                  {post.category ?? t('common.noData')}
                </span>
              </div>

              {post.tags.length > 0 ? (
                <div>
                  <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                    {t('forum.mod.pendingDetail.tags')}
                  </p>
                  <div className="flex flex-wrap gap-1">
                    {post.tags.map((tag) => (
                      <span key={tag} className="rounded border border-slate-200 bg-slate-50 px-2 py-0.5 text-xs text-slate-700">
                        {tag}
                      </span>
                    ))}
                  </div>
                </div>
              ) : null}

              <div>
                <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-slate-500">
                  {t('forum.mod.pendingDetail.content')}
                </p>
                <div className="rounded border border-slate-200 bg-white p-3 text-sm leading-relaxed text-slate-800">
                  {parsedContent.body ? (
                    <p className="whitespace-pre-line">{parsedContent.body}</p>
                  ) : null}
                  {parsedContent.imageUrls.length > 0 ? (
                    <div className={parsedContent.body ? 'mt-3' : ''}>
                      <p className="mb-2 text-[11px] font-medium uppercase tracking-wide text-slate-500">
                        {t('forum.detail.attachmentsLabel')}
                      </p>
                      <div className="grid grid-cols-1 gap-2">
                        {parsedContent.imageUrls.map((url) => (
                          <a key={url} href={url} target="_blank" rel="noopener noreferrer" className="block">
                            <img
                              src={url}
                              alt={t('forum.detail.attachmentImageAlt')}
                              loading="lazy"
                              className="max-h-72 w-full rounded-md border border-slate-200 object-contain bg-slate-50"
                            />
                          </a>
                        ))}
                      </div>
                    </div>
                  ) : null}
                  {parsedContent.fileUrls.length > 0 ? (
                    <div className="mt-3 space-y-1">
                      <p className="mb-1 text-[11px] font-medium uppercase tracking-wide text-slate-500">
                        {t('forum.detail.attachmentsLabel')}
                      </p>
                      {parsedContent.fileUrls.map((url) => (
                        <a
                          key={url}
                          href={url}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="block break-all text-xs text-primary hover:underline"
                        >
                          {url}
                        </a>
                      ))}
                    </div>
                  ) : null}
                  {!parsedContent.body &&
                  parsedContent.imageUrls.length === 0 &&
                  parsedContent.fileUrls.length === 0 ? (
                    <span className="text-slate-500">—</span>
                  ) : null}
                </div>
              </div>

              {showAi ? (
                <div className="rounded-lg border border-slate-200 bg-slate-50/80 p-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => void onAiHint()}
                      className="rounded-md border border-slate-800 bg-slate-900 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                    >
                      {isAiLoading ? t('forum.mod.pendingDetail.aiAnalyzing') : t('forum.mod.pendingDetail.aiHint')}
                    </button>
                    <p className="text-[11px] text-slate-500">{t('forum.mod.pendingDetail.aiDisclaimer')}</p>
                  </div>
                  {aiError ? <p className="mt-2 text-xs text-rose-700">{aiError}</p> : null}
                  {aiHint ? (
                    <div className="mt-3 space-y-2 border-t border-slate-200 pt-3 text-sm">
                      <p>
                        <span className="font-medium text-slate-700">{t('forum.mod.pendingDetail.recommendation')}: </span>
                        <span className="font-semibold text-slate-900">
                          {recommendationLabel(aiHint.recommendation, t)}
                        </span>
                      </p>
                      <p className="text-slate-700">
                        <span className="font-medium">{t('forum.mod.pendingDetail.reason')}: </span>
                        {aiHint.reason}
                      </p>
                      {aiHint.violations.length > 0 ? (
                        <div>
                          <p className="mb-1 font-medium text-slate-700">{t('forum.mod.pendingDetail.violations')}</p>
                          <ul className="list-inside list-disc space-y-1 text-xs text-slate-600">
                            {aiHint.violations.map((v, i) => (
                              <li key={`${v.type}-${i}`}>
                                {v.description}
                                {typeof v.severity === 'number' ? ` (${v.severity})` : ''}
                              </li>
                            ))}
                          </ul>
                        </div>
                      ) : null}
                    </div>
                  ) : null}
                </div>
              ) : null}
            </div>
          )}
        </div>

        <footer className="flex shrink-0 flex-wrap items-center justify-end gap-2 border-t border-slate-200 bg-white px-4 py-3">
          <a
            href={`/forum/${postId}`}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-1 rounded-md border border-slate-200 bg-white px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-50"
          >
            <ExternalLink className="h-3.5 w-3.5" />
            {t('forum.mod.pendingDetail.openInForum')}
          </a>
          <button
            type="button"
            disabled={busy || isLoading || !post}
            onClick={() => onApprove(postId)}
            className="rounded-md border border-slate-800 bg-slate-900 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-800 disabled:opacity-50"
          >
            {t('forum.mod.actions.approve')}
          </button>
        </footer>
      </div>
    </div>
  )
}
