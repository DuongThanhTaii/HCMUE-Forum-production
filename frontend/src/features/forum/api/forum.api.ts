import { baseApi } from '@shared/lib/api/baseApi'
import type { ForumPost } from '@shared/types/forum'

type ApiEnvelope<T> = {
  success?: boolean
  message?: string
  data?: T
}

type PostsPayload = {
  posts?: Array<{
    id: string
    title: string
    authorId?: string
    createdAt: string
    voteScore?: number
    commentCount: number
  }>
}

export const forumApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getPosts: builder.query<ForumPost[], void>({
      // Backend route is /api/v1/posts (PostsController), not /api/v1/forum/posts.
      query: () => '/api/v1/posts',
      transformResponse: (response: ApiEnvelope<PostsPayload>) =>
        (response?.data?.posts ?? []).map((post) => ({
          id: post.id,
          title: post.title,
          authorId: post.authorId,
          authorName: post.authorId ? `User ${post.authorId.slice(0, 8)}` : 'Unknown author',
          createdAt: post.createdAt,
          voteScore: post.voteScore ?? 0,
          voteCount: post.voteScore ?? 0,
          commentCount: post.commentCount ?? 0,
        })),
      providesTags: (result) =>
        result
          ? [
              ...result.map((post) => ({ type: 'ForumPost' as const, id: post.id })),
              { type: 'ForumPost' as const, id: 'LIST' },
            ]
          : [{ type: 'ForumPost' as const, id: 'LIST' }],
    }),
  }),
})

export const { useGetPostsQuery } = forumApi
