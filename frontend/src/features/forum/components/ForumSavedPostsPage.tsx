import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useGetBookmarkedForumListQuery } from '../api/forum.list.api'
import { ForumListTable } from './ForumListTable'

export function ForumSavedPostsPage() {
  const { t } = useTranslation()
  const { data: items = [], isLoading, isError } = useGetBookmarkedForumListQuery({
    pageNumber: 1,
    pageSize: 30,
  })

  if (isLoading) {
    return (
      <div className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
        {t('common.loading')}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="forum-compact-card border-rose-200 bg-rose-50 px-4 py-3 text-[14px] text-jasper">
        {t('forum.saved.loadFailed')}
      </div>
    )
  }

  if (items.length === 0) {
    return (
      <div className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
        {t('forum.saved.empty')}
      </div>
    )
  }

  return (
    <div className="space-y-2.5">
      <div className="forum-compact-card px-4 py-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h1 className="text-[16px] font-semibold text-slate-900">{t('forum.saved.title')}</h1>
          <Link to="/forum" className="text-[13px] font-medium text-primary hover:underline">
            {t('forum.saved.backToForum')}
          </Link>
        </div>
      </div>
      <ForumListTable items={items} />
    </div>
  )
}
