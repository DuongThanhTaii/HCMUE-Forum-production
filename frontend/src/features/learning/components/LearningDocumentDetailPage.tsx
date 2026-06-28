import { Link } from 'react-router-dom'
import { useLearningDocumentDetailPage } from '../hooks/useLearningDocumentDetailPage'
import {
  documentStatusBadgeClass,
  getDocumentStatusLabel,
  getDocumentStatusTone,
} from '../lib/documentStatus'

export function LearningDocumentDetailPage() {
  const {
    t,
    doc,
    isLoading,
    isError,
    userId,
    rating,
    setRating,
    ratingSending,
    downloadSending,
    rateMsg,
    downloadMsg,
    formatDate,
    fileSizeLabel,
    onSubmitRating,
    onDownload,
    onShareFacebook,
  } = useLearningDocumentDetailPage()

  if (isLoading) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-4 text-[14px] text-slate-600">
        {t('common.loading')}
      </div>
    )
  }

  if (isError || !doc) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-[14px] text-rose-700">
        {t('learning.messages.notFound')}
      </div>
    )
  }

  const statusLabel = getDocumentStatusLabel(doc.status, t)
  const statusClass = documentStatusBadgeClass(getDocumentStatusTone(doc.status))

  return (
    <div className="space-y-4">
      <div className="text-[13px] text-slate-500">
        <Link to="/learning/documents" className="text-primary hover:underline">
          ← {t('learning.documentList')}
        </Link>
      </div>

      <article className="forum-compact-card space-y-4 p-4">
        <header>
          <h1 className="text-lg font-semibold text-slate-900">{doc.title}</h1>
          <p className="mt-1 text-sm text-slate-500">
            {formatDate(doc.createdAt)} · {doc.documentType} ·{' '}
            <span className={`font-medium ${statusClass}`}>{statusLabel}</span>
          </p>
          <p className="mt-3 text-sm leading-relaxed text-slate-700">{doc.description}</p>
        </header>

        <section className="border-t border-slate-100 pt-4">
          <h2 className="text-sm font-semibold text-slate-800">{t('learning.documentDetailPage.documentInfo')}</h2>
          <dl className="mt-2 grid gap-2 text-[13px] sm:grid-cols-2">
            <div>
              <dt className="text-slate-500">{t('learning.documentDetailPage.fileName')}</dt>
              <dd className="font-medium text-slate-900">{doc.fileName}</dd>
            </div>
            <div>
              <dt className="text-slate-500">{t('learning.documentDetailPage.fileSize')}</dt>
              <dd className="font-medium text-slate-900">{fileSizeLabel}</dd>
            </div>
            <div>
              <dt className="text-slate-500">{t('learning.documentDetailPage.uploadDate')}</dt>
              <dd className="font-medium text-slate-900">{formatDate(doc.createdAt)}</dd>
            </div>
            <div>
              <dt className="text-slate-500">{t('learning.documentDetailPage.uploader')}</dt>
              <dd className="font-medium text-slate-900">
                {doc.uploaderDisplayName?.trim() || doc.uploaderId}
              </dd>
            </div>
            {doc.courseId ? (
              <div className="sm:col-span-2">
                <dt className="text-slate-500">{t('learning.documentDetailPage.course')}</dt>
                <dd className="font-medium text-slate-900">
                  {doc.courseName?.trim() || doc.courseId}
                </dd>
              </div>
            ) : null}
          </dl>
        </section>

        <section className="flex flex-wrap gap-4 border-t border-slate-100 pt-4 text-[13px] text-slate-600">
          <span>
            {t('learning.documentCard.downloadsLabel')}: <strong>{doc.downloadCount}</strong>
          </span>
          <span>
            {t('learning.documentCard.ratingsLabel')}: <strong>{doc.averageRating.toFixed(1)}</strong> (
            {doc.ratingCount})
          </span>
          <span>
            {t('learning.documentCard.views')}: <strong>{doc.viewCount}</strong>
          </span>
        </section>

        <section className="space-y-3 border-t border-slate-100 pt-4">
          <p className="text-[12px] text-slate-500">{t('learning.documentDetailPage.downloadHint')}</p>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={!userId || downloadSending}
              onClick={onDownload}
              className="rounded-md border border-primary bg-primary px-3 py-1.5 text-[13px] font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
            >
              {downloadSending ? t('common.loading') : t('learning.documentDetailPage.downloadDocument')}
            </button>
            <button
              type="button"
              onClick={onShareFacebook}
              className="rounded-md border border-sky-200 px-3 py-1.5 text-[13px] font-medium text-sky-700 hover:border-sky-400 hover:text-sky-800"
            >
              Facebook
            </button>
          </div>
          {downloadMsg ? (
            <p className={downloadMsg.kind === 'ok' ? 'text-sm text-emerald-700' : 'text-sm text-rose-700'}>
              {downloadMsg.text}
            </p>
          ) : null}
        </section>

        <section className="space-y-3 border-t border-slate-100 pt-4">
          <h2 className="text-sm font-semibold text-slate-800">{t('learning.documentDetailPage.rateDocument')}</h2>
          <div
            className="flex flex-wrap items-center gap-2"
            role="group"
            aria-label={t('learning.documentDetailPage.rateDocument')}
          >
            <span className="text-[13px] text-slate-600" id="rating-label">
              {t('learning.documentDetailPage.yourRating')}:
            </span>
            {[1, 2, 3, 4, 5].map((n) => (
              <button
                key={n}
                type="button"
                onClick={() => setRating(n)}
                aria-pressed={rating === n}
                aria-labelledby="rating-label"
                aria-label={t('learning.documentDetailPage.starRatingAria', { count: n })}
                className={`h-9 w-9 rounded-md border text-[13px] font-semibold transition-colors ${
                  rating === n
                    ? 'border-primary bg-primary/10 text-primary'
                    : 'border-slate-200 bg-white text-slate-600 hover:border-primary'
                }`}
              >
                {n}
              </button>
            ))}
          </div>
          <button
            type="button"
            disabled={!userId || ratingSending}
            onClick={onSubmitRating}
            className="rounded-md border border-slate-200 bg-white px-3 py-1.5 text-[13px] font-medium text-slate-800 hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-50"
          >
            {ratingSending ? t('learning.documentDetailPage.sendingRating') : t('learning.documentDetailPage.submitRating')}
          </button>
          {!userId ? (
            <p className="text-[13px] text-amber-700">{t('learning.messages.loginToInteract')}</p>
          ) : null}
          {rateMsg ? (
            <p className={rateMsg.kind === 'ok' ? 'text-sm text-emerald-700' : 'text-sm text-rose-700'}>
              {rateMsg.text}
            </p>
          ) : null}
        </section>
      </article>
    </div>
  )
}
