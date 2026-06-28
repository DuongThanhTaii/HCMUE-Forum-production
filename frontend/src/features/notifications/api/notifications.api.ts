import { baseApi } from '@shared/lib/api/baseApi'

export type NotificationDto = {
  id: string
  subject: string
  body: string
  actionUrl: string | null
  iconUrl: string | null
  status: string
  channel: string
  createdAt: string
  readAt: string | null
  isRead: boolean
}

export type BroadcastHomeAnnouncementRequest = {
  title: string
  message: string
  sendEmail: boolean
}

type GetNotificationsResponse = {
  notifications: NotificationDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export const notificationsApi = baseApi.injectEndpoints({
  endpoints: (b) => ({
    getNotifications: b.query<GetNotificationsResponse, { pageNumber?: number; pageSize?: number }>({
      query: ({ pageNumber = 1, pageSize = 20 } = {}) =>
        `/api/v1/notifications?pageNumber=${pageNumber}&pageSize=${pageSize}`,
      transformResponse: (r: { data: GetNotificationsResponse }) => r.data,
      providesTags: [{ type: 'Notification', id: 'LIST' }],
    }),
    getUnreadCount: b.query<number, void>({
      query: () => '/api/v1/notifications/unread-count',
      transformResponse: (r: { data: { count: number } }) => r.data.count,
      providesTags: [{ type: 'Notification', id: 'UNREAD_COUNT' }],
    }),
    markAsRead: b.mutation<void, string>({
      query: (id) => ({ url: `/api/v1/notifications/${id}/read`, method: 'PATCH' }),
      invalidatesTags: [{ type: 'Notification', id: 'LIST' }, { type: 'Notification', id: 'UNREAD_COUNT' }],
    }),
    markAllAsRead: b.mutation<void, void>({
      query: () => ({ url: '/api/v1/notifications/read-all', method: 'POST' }),
      invalidatesTags: [{ type: 'Notification', id: 'LIST' }, { type: 'Notification', id: 'UNREAD_COUNT' }],
    }),
    broadcastHomeAnnouncement: b.mutation<
      { sentInApp: number; sentEmail: number },
      BroadcastHomeAnnouncementRequest
    >({
      query: (body) => ({
        url: '/api/v1/notifications/broadcast/home',
        method: 'POST',
        body,
      }),
      transformResponse: (r: { data: { sentInApp: number; sentEmail: number } }) => r.data,
      invalidatesTags: [{ type: 'Notification', id: 'LIST' }, { type: 'Notification', id: 'UNREAD_COUNT' }],
    }),
  }),
})

export const {
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkAsReadMutation,
  useMarkAllAsReadMutation,
  useBroadcastHomeAnnouncementMutation,
} = notificationsApi
