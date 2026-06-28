import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { BookOpen, MessageSquare, MessagesSquare, PenSquare } from 'lucide-react'
import { useForumIndexPage, type CategoryLatestPost } from '../hooks/useForumIndexPage'
import type { ForumCategoryOption } from '../api/forum.list.api'
import {
  formatForumCount,
  getCategoryAccentStyles,
  getCategoryBorderAccent,
  getCategoryGroupIcon,
  type ForumCategoryGroup,
} from '../lib/forumCategoryTree'

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const m = Math.floor(diff / 60_000)
  if (m < 1) return 'vừa xong'
  if (m < 60) return `${m} phút`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h} giờ`
  return `${Math.floor(h / 24)} ngày`
}

function CategoryRow({
  category,
  latest,
  threadCount,
}: {
  category: ForumCategoryOption
  latest?: CategoryLatestPost
  threadCount: number
}) {
  const { t } = useTranslation()
  const messageCount = latest?.messageEstimate ?? 0

  return (
    <div className="group grid grid-cols-1 gap-2 border-b border-slate-100 px-3 py-2.5 transition-colors last:border-b-0 hover:bg-slate-50/90 md:grid-cols-[minmax(0,1.4fr)_5rem_5rem_minmax(0,1fr)] md:items-center md:gap-3 md:px-4">
      <div className="flex min-w-0 items-center gap-3">
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md border border-slate-200 bg-white text-slate-500 group-hover:border-primary/30 group-hover:text-primary">
          <MessageSquare className="h-3.5 w-3.5" aria-hidden />
        </div>
        <div className="min-w-0">
          <Link
            to={`/forum/posts?category=${category.id}`}
            className="text-[13px] font-semibold text-slate-900 hover:text-primary"
          >
            {category.name}
          </Link>
          {category.description ? (
            <p className="mt-0.5 line-clamp-1 text-[11px] text-slate-500">{category.description}</p>
          ) : null}
        </div>
      </div>

      <p className="text-center text-[12px] tabular-nums text-slate-700 md:text-right">
        <span className="font-semibold text-slate-900">{formatForumCount(threadCount)}</span>
        <span className="ml-1 text-slate-400 md:hidden">{t('forum.index.threads')}</span>
      </p>

      <p className="text-center text-[12px] tabular-nums text-slate-700 md:text-right">
        <span className="font-semibold text-slate-900">{formatForumCount(messageCount)}</span>
        <span className="ml-1 text-slate-400 md:hidden">{t('forum.index.messages')}</span>
      </p>

      <div className="min-w-0 border-t border-slate-100 pt-2 md:border-t-0 md:pt-0 md:text-right">
        {latest ? (
          <>
            <Link
              to={`/forum/${latest.post.id}`}
              className="block truncate text-[12px] font-medium text-primary hover:underline"
              title={latest.post.title}
            >
              {latest.post.title}
            </Link>
            <p className="mt-0.5 text-[10px] text-slate-400">{timeAgo(latest.post.activityAt)}</p>
          </>
        ) : (
          <p className="text-[11px] italic text-slate-400">{t('forum.index.noPostsYet')}</p>
        )}
      </div>
    </div>
  )
}

function GroupBlock({
  group,
  latestByCategoryId,
  postCountByCategoryId,
}: {
  group: ForumCategoryGroup
  latestByCategoryId: Map<string, CategoryLatestPost>
  postCountByCategoryId: Map<string, number>
}) {
  const { t } = useTranslation()
  const styles = getCategoryAccentStyles(group.accent)
  const borderAccent = getCategoryBorderAccent(group.accent)
  const Icon = getCategoryGroupIcon(group.iconKey)

  return (
    <section className={`forum-compact-card overflow-hidden border-l-4 ${borderAccent}`}>
      <header className={`flex items-start gap-3 border-b border-slate-200 px-4 py-3 ${styles.header}`}>
        <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg shadow-sm ${styles.icon}`}>
          <Icon className="h-5 w-5" aria-hidden />
        </div>
        <div>
          <h2 className="text-base font-bold tracking-tight text-slate-900">{group.parent.name}</h2>
          <p className="mt-0.5 text-xs leading-relaxed text-slate-600">{group.parent.description}</p>
        </div>
      </header>

      <div className="hidden border-b border-slate-200 bg-slate-50/90 px-4 py-2 text-[10px] font-semibold uppercase tracking-wide text-slate-500 md:grid md:grid-cols-[minmax(0,1.4fr)_5rem_5rem_minmax(0,1fr)] md:gap-3">
        <span>{t('forum.index.colForum')}</span>
        <span className="text-center">{t('forum.index.threads')}</span>
        <span className="text-center">{t('forum.index.messages')}</span>
        <span className="text-right">{t('forum.index.latest')}</span>
      </div>

      <div className="divide-y divide-slate-100">
        {group.children.map((cat) => (
          <CategoryRow
            key={cat.id}
            category={cat}
            latest={latestByCategoryId.get(cat.id)}
            threadCount={postCountByCategoryId.get(cat.id) ?? 0}
          />
        ))}
      </div>
    </section>
  )
}

export function ForumIndexPage() {
  const { t } = useTranslation()
  const { groups, latestByCategoryId, postCountByCategoryId, totals, isLoading, isError, isEmpty } =
    useForumIndexPage()

  if (isLoading) {
    return (
      <div className="forum-compact-card flex items-center gap-2 px-4 py-6 text-sm text-slate-600">
        <div className="h-4 w-4 animate-spin rounded-full border-2 border-slate-200 border-b-primary" />
        {t('common.loading')}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="forum-compact-card border-rose-200 bg-rose-50 px-4 py-3 text-sm text-jasper">
        {t('forum.error.loadFailed')}
      </div>
    )
  }

  if (isEmpty) {
    return (
      <div className="forum-compact-card px-4 py-8 text-center text-sm text-slate-500">
        {t('forum.empty.noPosts')}
      </div>
    )
  }

  return (
    <div className="space-y-5">
      <header className="forum-compact-card overflow-hidden border border-slate-200 shadow-sm">
        <div className="border-b border-slate-200 bg-gradient-to-br from-primary/8 via-white to-slate-50 px-4 py-4 md:px-6">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-[11px] font-bold uppercase tracking-widest text-primary">
                {t('forum.index.eyebrow')}
              </p>
              <h1 className="mt-1 text-2xl font-bold text-slate-900">{t('forum.index.title')}</h1>
              <p className="mt-2 max-w-2xl text-sm leading-relaxed text-slate-600">
                {t('forum.index.subtitle')}
              </p>
            </div>
            <div className="flex flex-wrap gap-2">
              <Link
                to="/forum/posts"
                className="inline-flex items-center gap-1.5 rounded-md border border-slate-300 bg-white px-3 py-2 text-[13px] font-medium text-slate-700 shadow-sm hover:border-primary hover:text-primary"
              >
                <MessagesSquare className="h-4 w-4" />
                {t('forum.index.viewAllPosts')}
              </Link>
              <Link
                to="/forum/new"
                className="inline-flex items-center gap-1.5 rounded-md border border-primary bg-primary px-3 py-2 text-[13px] font-medium text-white shadow-sm hover:bg-primary-hover"
              >
                <PenSquare className="h-4 w-4" />
                {t('forum.createPost.pageTitle')}
              </Link>
            </div>
          </div>
          <dl className="mt-5 grid max-w-lg grid-cols-2 gap-2 sm:grid-cols-3">
            <div className="rounded-lg border border-slate-200 bg-white px-3 py-2 shadow-sm">
              <dt className="text-[10px] font-semibold uppercase text-slate-500">{t('forum.index.statsZones')}</dt>
              <dd className="text-xl font-bold tabular-nums text-slate-900">{totals.zoneCount}</dd>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white px-3 py-2 shadow-sm">
              <dt className="text-[10px] font-semibold uppercase text-slate-500">
                {t('forum.index.statsCategories')}
              </dt>
              <dd className="text-xl font-bold tabular-nums text-slate-900">{totals.categoryCount}</dd>
            </div>
            <div className="col-span-2 rounded-lg border border-slate-200 bg-white px-3 py-2 shadow-sm sm:col-span-1">
              <dt className="text-[10px] font-semibold uppercase text-slate-500">{t('forum.index.statsThreads')}</dt>
              <dd className="text-xl font-bold tabular-nums text-slate-900">
                {formatForumCount(totals.threadCount)}
              </dd>
            </div>
          </dl>
        </div>
        <div className="grid sm:grid-cols-2">
          <Link
            to="/learning/documents"
            className="flex items-center gap-3 border-b border-slate-100 px-4 py-3 transition-colors hover:bg-indigo-50/50 sm:border-b-0 sm:border-r"
          >
            <BookOpen className="h-4 w-4 text-indigo-600" />
            <span className="text-sm font-medium text-slate-800">{t('forum.index.learningShortcut')}</span>
          </Link>
          <Link
            to="/forum/threads"
            className="flex items-center gap-3 px-4 py-3 transition-colors hover:bg-emerald-50/50"
          >
            <MessageSquare className="h-4 w-4 text-emerald-600" />
            <span className="text-sm font-medium text-slate-800">{t('forum.index.threadsShortcut')}</span>
          </Link>
        </div>
      </header>

      {groups.map((group) => (
        <GroupBlock
          key={group.parent.id}
          group={group}
          latestByCategoryId={latestByCategoryId}
          postCountByCategoryId={postCountByCategoryId}
        />
      ))}
    </div>
  )
}
