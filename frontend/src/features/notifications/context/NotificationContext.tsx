import {
  createContext,
  startTransition,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react'
import * as signalR from '@microsoft/signalr'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { baseApi } from '@shared/lib/api/baseApi'
import { useAppDispatch } from '@shared/hooks/useAppDispatch'

const NOTIFICATION_HUB_URL = `${(import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? 'http://localhost:5034'}/hubs/notifications`

export type LiveNotification = {
  id: string
  title: string
  message: string
  type: string
  createdAt: string
  isRead: boolean
  actionUrl?: string | null
}

type NotificationContextValue = {
  unreadCount: number
  liveItems: LiveNotification[]
  clearLive: (id: string) => void
  clearAllLive: () => void
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined)

function parseHubPayload(payload: unknown): LiveNotification | null {
  const p = payload as {
    id?: string
    title?: string
    message?: string
    type?: string
    createdAt?: string
    data?: { actionUrl?: string }
  }
  if (!p?.id || !p.title) return null
  const actionUrl =
    typeof p.data?.actionUrl === 'string' ? p.data.actionUrl : null
  return {
    id: p.id,
    title: p.title,
    message: p.message ?? '',
    type: p.type ?? 'info',
    createdAt: p.createdAt ?? new Date().toISOString(),
    isRead: false,
    actionUrl,
  }
}

function hubOn(
  conn: signalR.HubConnection,
  event: string,
  handler: (...args: unknown[]) => void,
) {
  conn.on(event, handler)
}

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const accessToken = useAppSelector((s) => s.auth.accessToken)
  const dispatch = useAppDispatch()
  const [unreadCount, setUnreadCount] = useState(0)
  const [liveItems, setLiveItems] = useState<LiveNotification[]>([])
  const connRef = useRef<signalR.HubConnection | null>(null)

  const invalidateNotifications = useCallback(() => {
    dispatch(
      baseApi.util.invalidateTags([
        { type: 'Notification', id: 'LIST' },
        { type: 'Notification', id: 'UNREAD_COUNT' },
      ]),
    )
  }, [dispatch])

  const onReceive = useCallback(
    (payload: unknown) => {
      const item = parseHubPayload(payload)
      if (!item) return
      startTransition(() => {
        setLiveItems((prev) => [item, ...prev.filter((x) => x.id !== item.id).slice(0, 49)])
        setUnreadCount((c) => c + 1)
      })
      invalidateNotifications()
    },
    [invalidateNotifications],
  )

  const onUnreadCount = useCallback(
    (count: number) => {
      startTransition(() => setUnreadCount(count))
      invalidateNotifications()
    },
    [invalidateNotifications],
  )

  useEffect(() => {
    if (!accessToken) {
      void connRef.current?.stop().catch(() => undefined)
      connRef.current = null
      setUnreadCount(0)
      setLiveItems([])
      return
    }

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(NOTIFICATION_HUB_URL, {
        accessTokenFactory: async () => accessToken,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10_000])
      .build()

    connRef.current = conn

    for (const evt of ['receiveNotification', 'ReceiveNotification'] as const) {
      hubOn(conn, evt, onReceive)
    }
    for (const evt of ['unreadCountUpdated', 'UnreadCountUpdated'] as const) {
      hubOn(conn, evt, (count: unknown) => {
        if (typeof count === 'number') onUnreadCount(count)
      })
    }

    void conn.start().catch(() => undefined)

    return () => {
      for (const evt of ['receiveNotification', 'ReceiveNotification'] as const) {
        conn.off(evt)
      }
      for (const evt of ['unreadCountUpdated', 'UnreadCountUpdated'] as const) {
        conn.off(evt)
      }
      void conn.stop().catch(() => undefined)
      connRef.current = null
    }
  }, [accessToken, onReceive, onUnreadCount])

  const clearLive = useCallback((id: string) => {
    setLiveItems((prev) => prev.filter((x) => x.id !== id))
  }, [])

  const clearAllLive = useCallback(() => setLiveItems([]), [])

  return (
    <NotificationContext.Provider value={{ unreadCount, liveItems, clearLive, clearAllLive }}>
      {children}
    </NotificationContext.Provider>
  )
}

export function useNotificationContext(): NotificationContextValue {
  const ctx = useContext(NotificationContext)
  if (!ctx) throw new Error('useNotificationContext must be inside NotificationProvider')
  return ctx
}
