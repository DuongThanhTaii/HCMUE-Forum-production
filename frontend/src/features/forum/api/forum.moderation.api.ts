import { baseApi } from '@shared/lib/api/baseApi'
import type {
  ModerationReportListResult,
  ModerationReportStatusFilter,
  PendingModerationPost,
} from '../types/forum-moderation'

type ApiEnvelope<T> = { data?: T; Data?: T }

type RawReportsPayload = {
  reports?: ModerationReportListResult['reports']
  totalCount?: number
  pageNumber?: number
  pageSize?: number
  totalPages?: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

type RawPostsPayload = {
  posts?: Array<{
    id: string
    title: string
    authorName?: string | null
    categoryName?: string | null
    createdAt: string
    commentCount: number
  }>
}

function unwrapData<T>(response: unknown): T | undefined {
  if (!response || typeof response !== 'object') return undefined
  const r = response as ApiEnvelope<T>
  return r.data ?? r.Data
}

export const forumModerationApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getModerationReports: builder.query<
      ModerationReportListResult,
      { status: ModerationReportStatusFilter; pageNumber?: number; pageSize?: number }
    >({
      query: ({ status, pageNumber = 1, pageSize = 20 }) => ({
        url: '/api/v1/mod/reports',
        params: { status, pageNumber, pageSize },
      }),
      transformResponse: (response: unknown): ModerationReportListResult => {
        const payload = unwrapData<RawReportsPayload>(response)
        return {
          reports: payload?.reports ?? [],
          totalCount: payload?.totalCount ?? 0,
          pageNumber: payload?.pageNumber ?? 1,
          pageSize: payload?.pageSize ?? 20,
          totalPages: payload?.totalPages ?? 0,
          hasPreviousPage: payload?.hasPreviousPage ?? false,
          hasNextPage: payload?.hasNextPage ?? false,
        }
      },
      providesTags: (result) =>
        result?.reports?.length
          ? [
              ...result.reports.map((r) => ({ type: 'ModerationReport' as const, id: r.id })),
              { type: 'ModerationReport' as const, id: 'LIST' },
            ]
          : [{ type: 'ModerationReport' as const, id: 'LIST' }],
    }),
    resolveModerationReport: builder.mutation<void, { reportId: number; action: 'keep' | 'remove' }>({
      query: ({ reportId, action }) => ({
        url: `/api/v1/mod/reports/${reportId}/resolve`,
        method: 'POST',
        body: { action },
      }),
      invalidatesTags: [
        { type: 'ModerationReport', id: 'LIST' },
        { type: 'ForumPost', id: 'LIST' },
      ],
    }),
    getModerationPendingPosts: builder.query<PendingModerationPost[], { pageNumber?: number; pageSize?: number; status?: number } | void>({
      query: (arg) => ({
        url: '/api/v1/mod/posts',
        params: { pageNumber: arg?.pageNumber ?? 1, pageSize: arg?.pageSize ?? 20, ...(typeof arg?.status === 'number' ? { status: arg.status } : {}) },
      }),
      transformResponse: (response: unknown): PendingModerationPost[] => {
        const payload = unwrapData<RawPostsPayload>(response)
        return (payload?.posts ?? []).map((p) => ({
          id: p.id,
          title: p.title,
          authorName: p.authorName ?? undefined,
          categoryName: p.categoryName ?? undefined,
          createdAt: p.createdAt,
          commentCount: p.commentCount,
        }))
      },
      providesTags: (result) =>
        result?.length
          ? [
              ...result.map((p) => ({ type: 'ModerationPost' as const, id: p.id })),
              { type: 'ModerationPost' as const, id: 'LIST' },
            ]
          : [{ type: 'ModerationPost' as const, id: 'LIST' }],
    }),
    rejectModerationPost: builder.mutation<void, { postId: string; reason: string }>({
      query: ({ postId, reason }) => ({
        url: `/api/v1/mod/posts/${postId}/reject`,
        method: 'PATCH',
        body: { reason },
      }),
      invalidatesTags: [
        { type: 'ModerationPost', id: 'LIST' },
        { type: 'ForumPost', id: 'LIST' },
      ],
    }),
    approveBulkModerationPosts: builder.mutation<void, { postIds: string[] }>({
      query: ({ postIds }) => ({
        url: `/api/v1/mod/posts/approve-bulk`,
        method: 'POST',
        body: { postIds },
      }),
      invalidatesTags: [
        { type: 'ModerationPost', id: 'LIST' },
        { type: 'ForumPost', id: 'LIST' },
      ],
    }),
    bulkDeleteModerationPosts: builder.mutation<void, { postIds: string[] }>({
      query: ({ postIds }) => ({
        url: `/api/v1/mod/posts/bulk-delete`,
        method: 'POST',
        body: { postIds },
      }),
      invalidatesTags: [
        { type: 'ModerationPost', id: 'LIST' },
        { type: 'ForumPost', id: 'LIST' },
      ],
    }),
  }),
});

export const {
  useGetModerationReportsQuery,
  useResolveModerationReportMutation,
  useGetModerationPendingPostsQuery,
  useRejectModerationPostMutation,
  useApproveBulkModerationPostsMutation,
  useBulkDeleteModerationPostsMutation,
} = forumModerationApi;
