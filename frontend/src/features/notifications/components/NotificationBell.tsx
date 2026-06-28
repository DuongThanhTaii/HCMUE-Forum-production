import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Bell, Check, ExternalLink } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useNotificationContext } from '../context/NotificationContext'
import {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkAllAsReadMutation,
  useMarkAsReadMutation,
} from '../api/notifications.api'

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const m = Math.floor(diff / 60_000)
  if (m < 1) return 'vừa xong'
  if (m < 60) return `${m} phút`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h} giờ`
  return `${Math.floor(h / 24)} ngày`
}

type DisplayItem = {
  id: string
  subject: string
  body: string
  createdAt: string
  isRead: boolean
  actionUrl?: string | null
  isLive?: boolean
}

export function NotificationBell() {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement | null>(null)
  const { unreadCount: liveUnread, liveItems, clearLive } = useNotificationContext()

  const { data: unreadData } = useGetUnreadCountQuery()
  const { data: notifs, isFetching } = useGetNotificationsQuery(
    { pageNumber: 1, pageSize: 15 },
    { skip: !open },
  )
  const [markRead] = useMarkAsReadMutation()
  const [markAllRead] = useMarkAllAsReadMutation()

  const count = Math.max(liveUnread, unreadData ?? 0)

  const displayItems = useMemo((): DisplayItem[] => {
    const fromApi: DisplayItem[] =
      notifs?.notifications?.map((n) => ({
        id: n.id,
        subject: n.subject,
        body: n.body,
        createdAt: n.createdAt,
        isRead: n.isRead,
        actionUrl: n.actionUrl,
      })) ?? []

    const apiIds = new Set(fromApi.map((n) => n.id))
    const liveOnly: DisplayItem[] = liveItems
      .filter((l) => !apiIds.has(l.id))
      .map((l) => ({
        id: l.id,
        subject: l.title,
        body: l.message,
        createdAt: l.createdAt,
        isRead: false,
        actionUrl: l.actionUrl,
        isLive: true,
      }))

    return [...liveOnly, ...fromApi]
  }, [notifs?.notifications, liveItems])

  useEffect(() => {
    if (!open) return
    const fn = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', fn)
    return () => document.removeEventListener('mousedown', fn)
  }, [open])

  const handleMarkAll = useCallback(async () => {
    await markAllRead()
  }, [markAllRead])

  const handleMarkOne = useCallback(
    async (id: string, isLive?: boolean) => {
      if (isLive) clearLive(id)
      await markRead(id)
    },
    [markRead, clearLive],
  )

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="relative flex h-8 w-8 items-center justify-center rounded-md text-slate-600 hover:bg-slate-100"
        aria-label={t('notifications.bell')}
      >
        <Bell className="h-4 w-4" />
        {count > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-0.5 text-[9px] font-bold text-white">
            {count > 99 ? '99+' : count}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 top-10 z-50 w-80 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-2.5">
            <span className="text-sm font-semibold text-slate-900">{t('notifications.title')}</span>
            {count > 0 && (
              <button
                type="button"
                onClick={() => void handleMarkAll()}
                className="flex items-center gap-1 text-[11px] text-indigo-600 hover:underline"
              >
                <Check className="h-3 w-3" />
                {t('notifications.markAllRead')}
              </button>
            )}
          </div>

          <div className="max-h-[min(60vh,380px)] overflow-y-auto">
            {isFetching && displayItems.length === 0 ? (
              <p className="py-6 text-center text-xs text-slate-400">{t('common.loading')}</p>
            ) : displayItems.length === 0 ? (
              <p className="py-8 text-center text-xs text-slate-400">{t('notifications.empty')}</p>
            ) : (
              <ul>
                {displayItems.map((n) => (
                  <li
                    key={n.id}
                    className={`flex gap-3 border-b border-slate-50 px-4 py-3 last:border-0 ${
                      n.isRead ? 'bg-white' : 'bg-indigo-50/60'
                    }`}
                  >
                    {!n.isRead && (
                      <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-indigo-500" />
                    )}
                    {n.isRead && <span className="mt-1.5 h-2 w-2 shrink-0" />}
                    <div className="min-w-0 flex-1">
                      <p className="text-xs font-medium leading-tight text-slate-900">{n.subject}</p>
                      <p className="mt-0.5 line-clamp-2 text-[11px] text-slate-500">{n.body}</p>
                      <div className="mt-1 flex items-center justify-between gap-2">
                        <span className="text-[10px] text-slate-400">{timeAgo(n.createdAt)}</span>
                        <div className="flex items-center gap-2">
                          {n.actionUrl && (
                            <Link
                              to={n.actionUrl}
                              onClick={() => {
                                setOpen(false)
                                if (!n.isRead) void handleMarkOne(n.id, n.isLive)
                              }}
                              className="inline-flex items-center gap-0.5 text-[11px] text-indigo-600 hover:underline"
                            >
                              {t('notifications.view')}
                              <ExternalLink className="h-2.5 w-2.5" />
                            </Link>
                          )}
                          {!n.isRead && (
                            <button
                              type="button"
                              onClick={() => void handleMarkOne(n.id, n.isLive)}
                              className="text-[11px] text-slate-400 hover:text-slate-600"
                            >
                              {t('notifications.markRead')}
                            </button>
                          )}
                        </div>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
