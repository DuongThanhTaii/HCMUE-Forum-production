import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import {
  BookOpen,
  BriefcaseBusiness,
  Flame,
  LayoutGrid,
  MessageSquarePlus,
  Sparkles,
  CalendarDays,
  Hash,
} from 'lucide-react'
import { useGetForumListQuery, useGetPopularForumTagsQuery } from '../api/forum.list.api'
import type { ForumListItem } from '../types/forum-list'
import { useGetDocumentsQuery } from '@features/learning/api/learning.api'
import { useGetJobsQuery } from '@features/career/api/career.api'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectIsAuthenticated } from '@features/auth/model/auth.slice'

const MS_DAY = 86_400_000

function parseTime(iso: string): number {
  const t = Date.parse(iso)
  return Number.isFinite(t) ? t : 0
}

function daysSince(iso: string): number {
  return (Date.now() - parseTime(iso)) / MS_DAY
}

function isWithinDays(iso: string, days: number): boolean {
  return daysSince(iso) >= 0 && daysSince(iso) <= days
}

/** Điểm “hot” phía client: tương tác + độ mới (MVP, sau có thể thay bằng API snapshot). */
function hotScore(post: ForumListItem): number {
  const d = Math.max(0, daysSince(post.activityAt))
  const freshness = Math.max(0, 10 - d)
  return post.replyCount * 5 + freshness * 2
}

export function StudentDashboard() {
  const { t } = useTranslation()
  const isAuthenticated = useAppSelector(selectIsAuthenticated)

  const { data: posts = [], isLoading: loadingPosts, isError: postsError } = useGetForumListQuery({
    pageNumber: 1,
    pageSize: 48,
  })
  const { data: popularTags = [], isLoading: loadingTags } = useGetPopularForumTagsQuery({ count: 14 })
  const { data: docsResult, isLoading: loadingDocs } = useGetDocumentsQuery({
    pageNumber: 1,
    pageSize: 5,
  })
  const { data: jobs = [], isLoading: loadingJobs } = useGetJobsQuery(
    { page: 1, pageSize: 5 },
    { skip: !isAuthenticated },
  )

  const documents = docsResult?.documents ?? []

  const hotPosts = useMemo(() => {
    const sorted = [...posts].sort((a, b) => hotScore(b) - hotScore(a))
    return sorted.slice(0, 6)
  }, [posts])

  const weekPosts = useMemo(() => {
    const inWeek = posts.filter((p) => isWithinDays(p.activityAt, 7))
    const sorted = [...inWeek].sort((a, b) => b.replyCount - a.replyCount || parseTime(b.activityAt) - parseTime(a.activityAt))
    return sorted.slice(0, 6)
  }, [posts])

  return (
    <div className="space-y-8">
      <header className="space-y-1">
        <h2 className="text-lg font-semibold text-slate-900">{t('home.studentDashboard.title')}</h2>
        <p className="text-sm text-slate-600">{t('home.studentDashboard.subtitle')}</p>
      </header>

      {postsError ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          {t('forum.error.loadFailed')}
        </div>
      ) : null}

      {/* Ba trụ cột: khám phá · tuần này · tag */}
      <div className="grid gap-4 lg:grid-cols-3">
        <section className="forum-compact-card overflow-hidden p-0">
          <div className="flex items-center gap-2 border-b border-orange-100 bg-gradient-to-r from-orange-50 to-amber-50 px-4 py-3">
            <Flame className="h-5 w-5 text-orange-600" aria-hidden />
            <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.hot.title')}</h3>
          </div>
          <ul className="divide-y divide-slate-100">
            {loadingPosts && posts.length === 0 ? (
              <li className="px-4 py-6 text-sm text-slate-500">{t('common.loading')}</li>
            ) : hotPosts.length === 0 ? (
              <li className="px-4 py-6 text-sm text-slate-500">{t('home.studentDashboard.hot.empty')}</li>
            ) : (
              hotPosts.map((post) => (
                <li key={post.id}>
                  <Link to={`/forum/${post.id}`} className="block px-4 py-3 transition hover:bg-slate-50">
                    <p className="line-clamp-2 text-sm font-medium text-slate-900">{post.title}</p>
                    <p className="mt-1 flex flex-wrap gap-2 text-[11px] text-slate-500">
                      <span>{post.category}</span>
                      <span>
                        {t('forum.replies')}: <strong className="text-slate-700">{post.replyCount}</strong>
                      </span>
                    </p>
                  </Link>
                </li>
              ))
            )}
          </ul>
        </section>

        <section className="forum-compact-card overflow-hidden p-0">
          <div className="flex items-center gap-2 border-b border-sky-100 bg-gradient-to-r from-sky-50 to-indigo-50 px-4 py-3">
            <CalendarDays className="h-5 w-5 text-sky-600" aria-hidden />
            <div>
              <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.week.title')}</h3>
              <p className="text-[11px] text-slate-500">{t('home.studentDashboard.week.hint')}</p>
            </div>
          </div>
          <ul className="divide-y divide-slate-100">
            {loadingPosts && posts.length === 0 ? (
              <li className="px-4 py-6 text-sm text-slate-500">{t('common.loading')}</li>
            ) : weekPosts.length === 0 ? (
              <li className="px-4 py-6 text-sm text-slate-500">{t('home.studentDashboard.week.empty')}</li>
            ) : (
              weekPosts.map((post) => (
                <li key={post.id}>
                  <Link to={`/forum/${post.id}`} className="block px-4 py-3 transition hover:bg-slate-50">
                    <p className="line-clamp-2 text-sm font-medium text-slate-900">{post.title}</p>
                    <p className="mt-1 text-[11px] text-slate-500">{post.threadChannelName ?? post.category}</p>
                  </Link>
                </li>
              ))
            )}
          </ul>
        </section>

        <section className="forum-compact-card overflow-hidden p-0">
          <div className="flex items-center gap-2 border-b border-violet-100 bg-gradient-to-r from-violet-50 to-fuchsia-50 px-4 py-3">
            <Hash className="h-5 w-5 text-violet-600" aria-hidden />
            <h3 className="text-sm font-semibold text-slate-900">{t('home.studentDashboard.tags.title')}</h3>
          </div>
          <div className="flex flex-wrap gap-2 px-4 py-4">
            {loadingTags && popularTags.length === 0 ? (
              <p className="text-sm text-slate-500">{t('common.loading')}</p>
            ) : popularTags.length === 0 ? (
              <p className="text-sm text-slate-500">{t('home.studentDashboard.tags.empty')}</p>
            ) : (
              popularTags.map((tag) => (
                <Link
                  key={tag.name}
                  to="/forum"
                  className="inline-flex items-center rounded-full border border-violet-200 bg-white px-3 py-1 text-xs font-medium text-violet-800 transition hover:bg-violet-50"
                >
                  #{tag.name}
                  <span className="ml-1.5 tabular-nums text-violet-600/80">{tag.postCount}</span>
                </Link>
              ))
            )}
          </div>
        </section>
      </div>

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
            to="/forum/threads"
            className="flex items-center gap-3 rounded-xl border border-slate-200 bg-white px-3 py-3 text-sm font-medium text-slate-800 transition hover:border-sky-300 hover:bg-sky-50/50"
          >
            <Sparkles className="h-5 w-5 shrink-0 text-sky-600" aria-hidden />
            {t('home.studentDashboard.shortcuts.threads')}
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
