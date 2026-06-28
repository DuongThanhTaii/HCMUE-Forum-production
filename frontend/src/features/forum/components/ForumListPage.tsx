import { Link } from 'react-router-dom'
import { ChevronLeft, PenSquare } from 'lucide-react'
import { ForumFiltersRow } from './ForumFiltersRow'
import { ForumListTable } from './ForumListTable'
import { useForumListPage } from '../hooks/useForumListPage'

export function ForumListPage() {
  const {
    t,
    activeTab,
    setActiveTab,
    filteredItems,
    isLoading,
    isError,
    isEmpty,
    activeCategory,
  } = useForumListPage()

  const toolbar = (
    <div className="flex flex-wrap items-center justify-between gap-3">
      <div className="min-w-0">
        <Link
          to="/forum"
          className="inline-flex items-center gap-1 text-[12px] font-medium text-slate-500 hover:text-primary"
        >
          <ChevronLeft className="h-3.5 w-3.5" />
          {t('forum.index.backToZones')}
        </Link>
        <h1 className="mt-1 truncate text-base font-semibold text-slate-900">
          {activeCategory ? activeCategory.name : t('forum.index.allPostsTitle')}
        </h1>
        {activeCategory?.description ? (
          <p className="mt-0.5 line-clamp-2 text-xs text-slate-500">{activeCategory.description}</p>
        ) : null}
      </div>
      <Link
        to="/forum/new"
        className="inline-flex shrink-0 items-center gap-2 rounded-md border border-primary bg-primary px-3 py-1.5 text-[13px] font-medium text-white transition-colors hover:bg-primary-hover"
      >
        <PenSquare className="h-3.5 w-3.5" aria-hidden />
        {t('forum.createPost.pageTitle')}
      </Link>
    </div>
  )

  if (isLoading) {
    return (
      <div className="space-y-2.5">
        {toolbar}
        <div className="forum-compact-card flex items-center gap-2 px-4 py-3 text-[14px] text-slate-600">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-slate-200 border-b-primary" />
          <span>{t('common.loading')}</span>
        </div>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-2.5">
        {toolbar}
        <div className="forum-compact-card border-rose-200 bg-rose-50 px-4 py-3 text-[14px] text-jasper">
          {t('forum.error.loadFailed')}
        </div>
      </div>
    )
  }

  if (isEmpty) {
    return (
      <div className="space-y-2.5">
        {toolbar}
        <div className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
          {t('forum.empty.noPosts')}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-2.5">
      {toolbar}
      <ForumFiltersRow activeTab={activeTab} onTabChange={setActiveTab} />
      <ForumListTable items={filteredItems} />
    </div>
  )
}
