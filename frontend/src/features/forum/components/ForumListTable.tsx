import type { ForumListItem } from '../types/forum-list'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

type ForumListTableProps = {
  items: ForumListItem[]
}

function formatActivityDate(activityAt: string) {
  const date = new Date(activityAt)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  })}`
}

export function ForumListTable({ items }: ForumListTableProps) {
  const { t } = useTranslation()

  return (
    <section className="forum-compact-card overflow-hidden divide-y divide-slate-100">
      <div className="sr-only">
        {t('forum.post')}, {t('forum.replies')}, {t('forum.meta.lastUpdated')}
      </div>

      {items.map((item) => (
        <article
          key={item.id}
          className="flex flex-col gap-2 px-4 py-3 transition-colors hover:bg-slate-50 sm:flex-row sm:items-start sm:justify-between sm:gap-4 md:gap-6"
        >
          <div className="min-w-0 flex-1">
            <h2 className="text-sm font-semibold leading-snug text-slate-900 sm:text-base">
              <Link
                to={`/forum/${item.id}`}
                className="forum-topic-link line-clamp-2 hover:text-primary sm:line-clamp-none"
              >
                {item.title}
              </Link>
            </h2>
            <p className="mt-1 line-clamp-2 text-xs text-slate-500 sm:line-clamp-1 sm:text-sm">
              <span className="text-slate-600">{item.category}</span>
              {item.tags.length > 0 && (
                <>
                  {' · '}
                  {item.tags.slice(0, 2).map((tag) => (
                    <span key={tag} className="forum-tag-chip mr-1 inline-block px-1.5 py-0.5 text-xs font-medium">
                      {tag}
                    </span>
                  ))}
                </>
              )}
            </p>
          </div>
          <div className="flex shrink-0 flex-wrap items-center gap-x-4 gap-y-1 text-xs text-slate-500 sm:flex-col sm:items-end sm:gap-1 md:flex-row md:items-center md:gap-6 md:text-sm">
            <p className="tabular-nums">
              <span className="sr-only">{t('forum.replies')}: </span>
              <span className="font-semibold text-slate-700">{item.replyCount}</span>
            </p>
            <p className="min-w-0 text-right">
              <span className="sr-only">{t('forum.meta.lastUpdated')}: </span>
              {formatActivityDate(item.activityAt) || t('common.noData')}
            </p>
          </div>
        </article>
      ))}
    </section>
  )
}
