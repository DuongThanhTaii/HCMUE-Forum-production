import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useGetForumListQuery, useGetForumThreadChannelsQuery } from '../api/forum.list.api'

export function ForumThreadsPage() {
  const { t } = useTranslation()
  const { data: channels = [], isLoading: isLoadingChannels, isError: isChannelsError } = useGetForumThreadChannelsQuery()
  const [activeChannelId, setActiveChannelId] = useState<string | null>(null)
  const selectedChannelId = activeChannelId ?? channels[0]?.id ?? null
  const { data = [], isLoading, isError } = useGetForumListQuery({
    pageNumber: 1,
    pageSize: 80,
    threadChannelId: selectedChannelId ?? undefined,
  })

  const threadTopics = useMemo(
    () => [...data].sort((a, b) => new Date(b.activityAt).getTime() - new Date(a.activityAt).getTime()),
    [data],
  )

  if (isLoadingChannels || isLoading) {
    return (
      <div className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
        {t('common.loading')}
      </div>
    )
  }

  if (isChannelsError || isError) {
    return (
      <div className="forum-compact-card border-rose-200 bg-rose-50 px-4 py-3 text-[14px] text-jasper">
        {t('forum.error.loadFailed')}
      </div>
    )
  }

  return (
    <div className="space-y-2.5">
      <section className="forum-compact-card px-4 py-3">
        <h1 className="text-[18px] font-semibold text-slate-900">{t('forum.threads.title')}</h1>
        <p className="mt-1 text-[13px] text-slate-600">{t('forum.threads.subtitle')}</p>
        {channels.length > 0 ? (
          <div className="mt-3 flex flex-wrap gap-2">
            {channels.map((channel) => {
              const active = channel.id === selectedChannelId
              return (
                <button
                  key={channel.id}
                  type="button"
                  onClick={() => setActiveChannelId(channel.id)}
                  className={`rounded-full border px-3 py-1 text-xs font-medium ${
                    active
                      ? 'border-primary bg-primary/10 text-primary'
                      : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300'
                  }`}
                >
                  {channel.name}
                </button>
              )
            })}
          </div>
        ) : null}
      </section>

      {threadTopics.length === 0 ? (
        <section className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
          {t('forum.threads.empty')}
        </section>
      ) : (
        <section className="forum-compact-card divide-y divide-slate-100 overflow-hidden">
          {threadTopics.map((topic) => (
            <article key={topic.id} className="px-4 py-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <h2 className="text-[15px] font-semibold text-slate-900">{topic.title}</h2>
                  <p className="mt-1 text-[13px] text-slate-600">
                    {t('forum.replies')}: <span className="font-semibold">{topic.replyCount}</span>
                  </p>
                </div>
                <Link
                  to={`/forum/${topic.id}?view=thread`}
                  className="rounded-md border border-primary px-3 py-1.5 text-[13px] font-medium text-primary hover:bg-primary/5"
                >
                  {t('forum.threads.openThread')}
                </Link>
              </div>
            </article>
          ))}
        </section>
      )}
    </div>
  )
}
