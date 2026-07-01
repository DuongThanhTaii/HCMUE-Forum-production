import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import {
  BookOpen,
  BriefcaseBusiness,
  LayoutGrid,
  MessageSquarePlus,
  Sparkles,
} from 'lucide-react'
import { useGetDocumentsQuery } from '@features/learning/api/learning.api'
import { useGetJobsQuery } from '@features/career/api/career.api'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectIsAuthenticated } from '@features/auth/model/auth.slice'

export function StudentDashboard() {
  const { t } = useTranslation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)

  const { data: docsResult, isLoading: loadingDocs } = useGetDocumentsQuery({
    pageNumber: 1,
    pageSize: 5,
  })
  const { data: jobs = [], isLoading: loadingJobs } = useGetJobsQuery(
    { page: 1, pageSize: 5 },
    { skip: !isAuthenticated },
  )

  const documents = docsResult?.documents ?? []

  return (
    <div className="space-y-8">
      <header className="space-y-1">
        <h2 className="text-lg font-semibold text-slate-900">{t('home.studentDashboard.title')}</h2>
        <p className="text-sm text-slate-600">{t('home.studentDashboard.subtitle')}</p>
      </header>

      {/* Only displaying shortcuts and other non-thread elements */}

      {/* Shortcut */}
      <section className="forum-compact-card p-4 md:p-5">
        <div className="mb-3 flex items-center gap-2">
          <LayoutGrid className="h-4 w-4 text-slate-500" aria-hidden />
          <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.shortcuts.title')}</h3>
        </div>
        <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          <Link
            to="/forum/new"
            className="flex items-center gap-3 rounded-xl border border-slate-200 bg-white px-3 py-3 text-sm font-medium text-slate-800 transition hover:border-primary hover:bg-primary/5"
          >
            <MessageSquarePlus className="h-5 w-5 shrink-0 text-primary" aria-hidden />
            {t('home.studentDashboard.shortcuts.newPost')}
          </Link>
          <Link
            to="/explore"
            className="flex items-center gap-3 rounded-xl border border-slate-200 bg-white px-3 py-3 text-sm font-medium text-slate-800 transition hover:border-sky-300 hover:bg-sky-50/50"
          >
            <Sparkles className="h-5 w-5 shrink-0 text-sky-600" aria-hidden />
            Explore Discussions
          </Link>
          <Link
            to="/learning/documents"
            className="flex items-center gap-3 rounded-xl border border-slate-200 bg-white px-3 py-3 text-sm font-medium text-slate-800 transition hover:border-emerald-300 hover:bg-emerald-50/50"
          >
            <BookOpen className="h-5 w-5 shrink-0 text-emerald-600" aria-hidden />
            {t('home.studentDashboard.shortcuts.learning')}
          </Link>
          <Link
            to="/career/jobs"
            className={`flex items-center gap-3 rounded-xl border border-slate-200 bg-white px-3 py-3 text-sm font-medium transition hover:border-amber-300 hover:bg-amber-50/50 ${
              isAuthenticated ? 'text-slate-800' : 'text-slate-400'
            }`}
          >
            <BriefcaseBusiness className="h-5 w-5 shrink-0 text-amber-700" aria-hidden />
            {t('home.studentDashboard.shortcuts.career')}
          </Link>
        </div>
        {!isAuthenticated ? (
          <p className="mt-3 text-[12px] text-slate-500">{t('home.studentDashboard.shortcuts.loginHint')}</p>
        ) : null}
      </section>

      {/* Tài liệu + việc làm */}
      <div className="grid gap-4 lg:grid-cols-2">
        <section className="forum-compact-card p-4 md:p-5">
          <div className="flex items-center justify-between gap-2">
            <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.docs.title')}</h3>
            <Link to="/learning/documents" className="text-xs font-medium text-primary hover:underline">
              {t('home.studentDashboard.docs.viewAll')}
            </Link>
          </div>
          <ul className="mt-3 space-y-2">
            {documents.length === 0 ? (
              <li className="text-sm text-slate-500">
                {loadingDocs ? t('common.loading') : t('home.studentDashboard.docs.empty')}
              </li>
            ) : (
              documents.map((doc) => (
                <li key={doc.id}>
                  <Link
                    to={`/learning/documents/${doc.id}`}
                    className="block rounded-lg border border-transparent px-2 py-2 text-sm font-medium text-slate-800 hover:border-emerald-100 hover:bg-emerald-50/40"
                  >
                    {doc.title}
                  </Link>
                </li>
              ))
            )}
          </ul>
        </section>

        <section className="forum-compact-card p-4 md:p-5">
          <div className="flex items-center justify-between gap-2">
            <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.jobs.title')}</h3>
            {isAuthenticated ? (
              <Link to="/career/jobs" className="text-xs font-medium text-primary hover:underline">
                {t('home.studentDashboard.jobs.viewAll')}
              </Link>
            ) : null}
          </div>
          {!isAuthenticated ? (
            <p className="mt-3 text-sm text-slate-500">{t('home.studentDashboard.jobs.loginPrompt')}</p>
          ) : loadingJobs ? (
            <p className="mt-3 text-sm text-slate-500">{t('common.loading')}</p>
          ) : jobs.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">{t('home.studentDashboard.jobs.empty')}</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {jobs.map((job) => (
                <li key={job.id}>
                  <Link
                    to={`/career/jobs/${job.id}`}
                    className="block rounded-lg border border-transparent px-2 py-2 text-sm font-medium text-slate-800 hover:border-amber-100 hover:bg-amber-50/40"
                  >
                    <span className="line-clamp-1">{job.title}</span>
                    <span className="mt-0.5 block text-[11px] font-normal text-slate-500">
                      {job.companyName ?? '—'} · {job.city ?? '—'}
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  )
}
