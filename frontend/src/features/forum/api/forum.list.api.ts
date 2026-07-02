import { baseApi } from '@shared/lib/api/baseApi'
import type { ForumListItem } from '../types/forum-list'

type ApiSuccessEnvelope<T> = {
  success?: boolean
  message?: string
  data?: T
}

type RawForumPost = {
  id?: string
  title?: string
  type?: number | null
  authorId?: string | null
  author_id?: string | null
  tags?: string[] | null
  categoryName?: string | null
  category?: { name?: string | null } | null
  categoryId?: string | null
  category_id?: string | null
  threadChannelId?: string | null
  thread_channel_id?: string | null
  threadChannelCode?: string | null
  threadChannelName?: string | null
  authorName?: string | null
  commentCount?: number | null
  comment_count?: number | null
  replyCount?: number | null
  voteScore?: number | null
  vote_score?: number | null
  isBookmarked?: boolean | null
  updatedAt?: string | null
  updated_at?: string | null
  publishedAt?: string | null
  published_at?: string | null
  createdAt?: string | null
  created_at?: string | null
  content?: string | null
  body?: string | null
  // New Thread Card fields
  preview?: string | null
  authorAvatar?: string | null
  viewCount?: number | null
  view_count?: number | null
  likeCount?: number | null
  bookmarkCount?: number | null
  bookmark_count?: number | null
  categorySlug?: string | null
  lastActivity?: string | null
  isPinned?: boolean | null
  isLocked?: boolean | null
  isSolved?: boolean | null
  currentUserVote?: number | null
  userVote?: number | null
  myVote?: number | null
  isUpvoted?: boolean | null
}

type RawForumComment = {
  id?: string
  postId?: string
  post_id?: string
  authorId?: string
  author_id?: string
  authorName?: string | null
  author_name?: string | null
  authorAvatar?: string
  author_avatar?: string
  authorRoles?: string[]
  content?: string
  parentCommentId?: string | null
  parent_comment_id?: string | null
  voteScore?: number
  vote_score?: number
  currentUserVote?: number | null
  userVote?: number | null
  myVote?: number | null
  isAcceptedAnswer?: boolean
  is_accepted_answer?: boolean
  isPinned?: boolean
  is_pinned?: boolean
  createdAt?: string
  created_at?: string
  updatedAt?: string | null
  updated_at?: string | null
}

type PostsPayload = {
  posts?: RawForumPost[]
  items?: RawForumPost[]
}

type CommentsPayload = {
  comments?: RawForumComment[]
  items?: RawForumComment[]
}

type ForumListQueryParams = {
  pageNumber?: number
  pageSize?: number
  threadChannelId?: string
  categoryId?: string
  searchTerm?: string
  sortBy?: number
  isSolved?: boolean
  isUnanswered?: boolean
  isPinned?: boolean
}

type VoteType = 1 | 2

export type ForumCommentItem = {
  id: string
  postId: string
  authorId: string
  authorName: string
  authorAvatar?: string
  authorRoles?: string[]
  content: string
  parentCommentId: string | null
  voteScore: number
  currentUserVote: 0 | 1 | 2
  isAcceptedAnswer: boolean
  isPinned: boolean
  createdAt: string
  updatedAt?: string
}

type AcceptAnswerRequest = {
  commentId: string
  postId: string
}

type PinCommentRequest = {
  commentId: string
  postId: string
}

type AddCommentRequest = {
  postId: string
  content: string
  parentCommentId?: string
}

type VotePostRequest = {
  postId: string
  voteType: VoteType
}

type VoteCommentRequest = {
  commentId: string
  postId: string
  voteType: VoteType
}

type ReportPostRequest = {
  postId: string
  reason: number
  description?: string
}

export type ForumCategoryOption = {
  id: string
  name: string
  description: string
  slug: string
  parentCategoryId?: string | null
  postCount: number
  displayOrder: number
}

export type ForumPopularTag = {
  name: string
  postCount: number
}

export type ForumThreadChannel = {
  id: string
  code: string
  name: string
  description: string
  displayOrder: number
  isActive: boolean
  allowPinnedComments: boolean
  allowAcceptedAnswers: boolean
  allowModeratorActions: boolean
}

type CreatePostRequest = {
  title: string
  content: string
  type: number
  categoryId?: string | null
  threadChannelId?: string | null
  tags?: string[]
}

type CreatePostResponseData = {
  postId?: string
}

type UploadForumAttachmentsResponse = {
  urls?: string[]
  Urls?: string[]
}

/** Single cache key for post detail + mutations (GUID casing from URL vs API was breaking invalidation). */
export function normalizeForumPostId(raw: string): string {
  if (typeof raw !== 'string') {
    return ''
  }
  return raw.trim().toLowerCase()
}

function toSafeForumListItem(post: RawForumPost, index: number): ForumListItem {
  const id =
    post.id && post.id.trim().length > 0 ? normalizeForumPostId(post.id) : `post-${index}`
  const title = post.title && post.title.trim().length > 0 ? post.title : 'Untitled post'
  const category =
    post.categoryName?.trim() ||
    post.category?.name?.trim() ||
    (post.categoryId ?? post.category_id ? `Category ${(post.categoryId ?? post.category_id!).slice(0, 8)}` : 'General')
  const tags = Array.isArray(post.tags) ? post.tags.filter((tag): tag is string => Boolean(tag?.trim())) : []
  const replyCount = Math.max(0, post.replyCount ?? post.commentCount ?? post.comment_count ?? 0)
  const activityAt = post.updatedAt || post.updated_at || post.publishedAt || post.published_at || post.createdAt || post.created_at || '1970-01-01T00:00:00.000Z'

  return {
    id,
    title,
    category,
    categorySlug: post.categorySlug?.trim() || undefined,
    categoryId: post.categoryId?.trim() || post.category_id?.trim() || undefined,
    threadChannelId: post.threadChannelId?.trim() || post.thread_channel_id?.trim() || undefined,
    threadChannelCode: post.threadChannelCode?.trim() || undefined,
    threadChannelName: post.threadChannelName?.trim() || undefined,
    tags,
    replyCount,
    likeCount: post.likeCount ?? post.voteScore ?? post.vote_score ?? 0,
    viewCount: post.viewCount ?? post.view_count ?? 0,
    bookmarkCount: post.bookmarkCount ?? post.bookmark_count ?? 0,
    authorId: post.authorId?.trim() || post.author_id?.trim() || undefined,
    authorName: post.authorName?.trim() || undefined,
    authorAvatar: post.authorAvatar?.trim() || undefined,
    preview: post.preview?.trim() || post.content?.trim()?.substring(0, 200) || '',
    isPinned: post.isPinned ?? false,
    isLocked: post.isLocked ?? false,
    isSolved: post.isSolved ?? false,
    activityAt: post.lastActivity || activityAt,
    isBookmarked: post.isBookmarked === true,
    isUpvoted: post.isUpvoted === true || post.currentUserVote === 1 || post.userVote === 1,
  }
}

export type ForumDetailItem = ForumListItem & {
  content?: string
  body?: string
  authorName?: string
  authorId?: string
  type?: number
  voteScore?: number
  isBookmarked?: boolean
  isUpvoted?: boolean
}

function toSafeForumDetailItem(post: RawForumPost, idFallback: string): ForumDetailItem {
  const base = toSafeForumListItem(post, 0)
  const id = post.id?.trim() ? normalizeForumPostId(post.id) : normalizeForumPostId(idFallback)
  const content = post.content?.trim() || undefined
  const body = post.body?.trim() || undefined
  const authorName = post.authorName?.trim() || undefined
  const authorId = post.authorId?.trim() || post.author_id?.trim() || undefined
  const type = typeof post.type === 'number' ? post.type : undefined
  const threadChannelId = post.threadChannelId?.trim() || post.thread_channel_id?.trim() || undefined
  const threadChannelCode = post.threadChannelCode?.trim() || undefined
  const threadChannelName = post.threadChannelName?.trim() || undefined
  const voteScore = typeof post.voteScore === 'number' ? post.voteScore : typeof post.vote_score === 'number' ? post.vote_score : undefined
  const isBookmarked = post.isBookmarked === true
  
  const rawVote = post.currentUserVote ?? post.userVote ?? post.myVote
  const isUpvoted = post.isUpvoted === true || rawVote === 1

  return {
    ...base,
    id,
    content,
    body,
    authorName,
    authorId,
    type,
    threadChannelId,
    threadChannelCode,
    threadChannelName,
    voteScore,
    isBookmarked,
    isUpvoted,
  }
}

function toSafeForumCommentItem(comment: RawForumComment, postIdFallback: string, index: number): ForumCommentItem {
  const id = comment.id?.trim() || `comment-${index}`
  const postId = comment.postId?.trim() || comment.post_id?.trim()
    ? normalizeForumPostId(comment.postId || comment.post_id!)
    : normalizeForumPostId(postIdFallback)
  const authorId = comment.authorId?.trim() || comment.author_id?.trim() || 'unknown-author'
  const content = comment.content?.trim() || ''
  const createdAt = comment.createdAt || comment.created_at || '1970-01-01T00:00:00.000Z'
  const parentRaw = comment.parentCommentId?.trim() || comment.parent_comment_id?.trim()
  const parentCommentId = parentRaw && parentRaw.length > 0 ? parentRaw : null
  const named = comment.authorName?.trim() || comment.author_name?.trim()
  const authorName =
    named && named.length > 0 ? named : `User ${authorId.slice(0, 8)}`
  const authorAvatar = comment.authorAvatar?.trim() || comment.author_avatar?.trim() || undefined
  const authorRoles = comment.authorRoles || []
  const rawCurrentVote = comment.currentUserVote ?? comment.userVote ?? comment.myVote ?? null
  const currentUserVote: 0 | 1 | 2 = rawCurrentVote === 1 ? 1 : rawCurrentVote === -1 || rawCurrentVote === 2 ? 2 : 0
  const isAcceptedAnswer = comment.isAcceptedAnswer === true || comment.is_accepted_answer === true
  const isPinned = comment.isPinned === true || comment.is_pinned === true
  const voteScore = comment.voteScore ?? comment.vote_score ?? 0

  return {
    id,
    postId,
    authorId,
    authorName,
    authorAvatar,
    authorRoles,
    content,
    parentCommentId,
    voteScore,
    currentUserVote,
    isAcceptedAnswer,
    isPinned,
    createdAt,
    updatedAt: comment.updatedAt ?? comment.updated_at ?? undefined,
  }
}

export const forumListApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getForumCategories: builder.query<ForumCategoryOption[], void>({
      query: () => '/api/v1/categories',
      transformResponse: (response: ApiSuccessEnvelope<any>) => {
        const rawData = response?.data
        const list = Array.isArray(rawData) ? rawData : rawData?.categories
        if (!Array.isArray(list)) return []
        const categories: ForumCategoryOption[] = []
        for (const c of list) {
          const row = c as {
            id?: string
            Id?: string
            name?: string
            Name?: string
            description?: string
            Description?: string
            slug?: string
            Slug?: string
            parentCategoryId?: string | null
            ParentCategoryId?: string | null
            postCount?: number
            PostCount?: number
            displayOrder?: number
            DisplayOrder?: number
          }
          const id =
            (typeof row.id === 'string' && row.id.trim()) ||
            (typeof row.Id === 'string' && row.Id.trim()) ||
            ''
          if (!id) continue
          const parentRaw = row.parentCategoryId ?? row.ParentCategoryId ?? (row as any).parent_category_id
          const postCountRaw = row.postCount ?? row.PostCount ?? (row as any).post_count
          const displayOrderRaw = row.displayOrder ?? row.DisplayOrder
          categories.push({
            id,
            name: (row.name ?? row.Name)?.trim() || 'Category',
            description: (row.description ?? row.Description)?.trim() || '',
            slug: (row.slug ?? row.Slug)?.trim() || id,
            parentCategoryId:
              typeof parentRaw === 'string' && parentRaw.trim() ? parentRaw.trim() : null,
            postCount: typeof postCountRaw === 'number' ? postCountRaw : 0,
            displayOrder: typeof displayOrderRaw === 'number' ? displayOrderRaw : 0,
          })
        }
        return categories
      },
      providesTags: [{ type: 'ForumCategory' as const, id: 'LIST' }],
    }),
    getForumThreadChannels: builder.query<ForumThreadChannel[], { includeInactive?: boolean } | void>({
      query: (arg) => ({
        url: arg && typeof arg === 'object' && arg.includeInactive ? '/api/v1/thread-channels/admin' : '/api/v1/thread-channels',
      }),
      transformResponse: (response: ApiSuccessEnvelope<{ channels?: unknown[] }>) => {
        const rows = response?.data?.channels
        if (!Array.isArray(rows)) return []
        return rows
          .map((row) => {
            const r = row as {
              id?: string
              code?: string
              name?: string
              description?: string | null
              displayOrder?: number
              isActive?: boolean
              allowPinnedComments?: boolean
              allowAcceptedAnswers?: boolean
              allowModeratorActions?: boolean
            }
            if (!r.id || !r.code || !r.name) return null
            return {
              id: r.id,
              code: r.code,
              name: r.name,
              description: r.description ?? '',
              displayOrder: typeof r.displayOrder === 'number' ? r.displayOrder : 0,
              isActive: r.isActive !== false,
              allowPinnedComments: r.allowPinnedComments !== false,
              allowAcceptedAnswers: r.allowAcceptedAnswers !== false,
              allowModeratorActions: r.allowModeratorActions !== false,
            } satisfies ForumThreadChannel
          })
          .filter((x): x is ForumThreadChannel => x !== null)
      },
      providesTags: [{ type: 'ForumCategory' as const, id: 'THREAD_CHANNELS' }],
    }),
    createForumPost: builder.mutation<string, CreatePostRequest>({
      query: (body) => ({
        url: '/api/v1/posts',
        method: 'POST',
        body: {
          title: body.title,
          content: body.content,
          type: body.type,
          categoryId: body.categoryId || undefined,
          threadChannelId: body.threadChannelId || undefined,
          tags: body.tags,
        },
      }),
      transformResponse: (response: ApiSuccessEnvelope<CreatePostResponseData>) => {
        const id = response?.data?.postId
        if (typeof id === 'string' && id.trim()) return normalizeForumPostId(id)
        return ''
      },
      invalidatesTags: ['ForumPost'],
    }),
    deleteComment: builder.mutation<void, { commentId: string; postId: string }>({
      query: ({ commentId }) => ({
        url: `/api/v1/comments/${commentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { postId }) => [
        { type: 'Comment' as const, id: `POST-${normalizeForumPostId(postId)}` },
      ],
    }),
    uploadForumAttachments: builder.mutation<string[], File[]>({
      query: (files) => {
        const formData = new FormData()
        for (const file of files) {
          formData.append('files', file)
        }
        return {
          url: '/api/v1/posts/attachments/upload',
          method: 'POST',
          body: formData,
        }
      },
      transformResponse: (response: ApiSuccessEnvelope<UploadForumAttachmentsResponse>) => {
        const payload = response?.data
        const urls = payload?.urls ?? payload?.Urls ?? []
        return Array.isArray(urls) ? urls.filter((x): x is string => typeof x === 'string' && x.trim().length > 0) : []
      },
    }),
    publishForumPost: builder.mutation<void, { postId: string }>({
      query: ({ postId }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/posts/${pid}/publish`,
          method: 'POST',
        }
      },
      invalidatesTags: (_r, _e, { postId }) => [
        { type: 'ForumPost', id: normalizeForumPostId(postId) },
        { type: 'ForumPost', id: 'LIST' },
        { type: 'ModerationPost', id: 'LIST' },
      ],
    }),
    getPopularForumTags: builder.query<ForumPopularTag[], { count?: number } | void>({
      query: (arg) => ({
        url: '/api/v1/tags/popular',
        params: { count: arg && typeof arg === 'object' ? (arg.count ?? 24) : 24 },
      }),
      transformResponse: (response: ApiSuccessEnvelope<unknown>) => {
        const raw = response?.data
        if (!Array.isArray(raw)) return []
        return raw
          .map((row) => {
            const r = row as { name?: string; Name?: string; postCount?: number; PostCount?: number; post_count?: number; usage_count?: number }
            const name = (r.name ?? r.Name ?? '').trim()
            if (!name) return null
            const postCountRaw = r.usage_count ?? r.postCount ?? r.PostCount ?? r.post_count
            const postCount = typeof postCountRaw === 'number' ? postCountRaw : Number(postCountRaw ?? 0) || 0
            return { name, postCount } satisfies ForumPopularTag
          })
          .filter((x): x is ForumPopularTag => x !== null)
      },
      providesTags: [{ type: 'ForumTag' as const, id: 'POPULAR' }],
    }),
    // FE vote value (1 up, 2 down) to score delta value (+1 / -1).
    // API currently returns current user vote as 1 or -1.
    // We normalize comment.currentUserVote to FE space (1 / 2 / 0).
    // This helper keeps optimistic updates consistent with Reddit-style toggle.
    getForumList: builder.query<ForumListItem[], ForumListQueryParams | undefined>({
      query: (params = {}) => ({
        // Backend controller route: [Route("api/v1/posts")] + [HttpGet]
        url: '/api/v1/posts',
        params: {
          pageNumber: params.pageNumber ?? 1,
          pageSize: params.pageSize ?? 20,
          threadChannelId: params.threadChannelId ?? undefined,
          categoryId: params.categoryId ?? undefined,
          searchTerm: params.searchTerm ?? undefined,
          sortBy: params.sortBy ?? undefined,
          isSolved: params.isSolved ?? undefined,
          isUnanswered: params.isUnanswered ?? undefined,
          isPinned: params.isPinned ?? undefined,
        },
      }),
      transformResponse: (response: ApiSuccessEnvelope<PostsPayload>) => {
        const payload = response?.data
        const posts = payload?.posts ?? payload?.items ?? []
        return posts.map(toSafeForumListItem)
      },
      providesTags: (result) =>
        result
          ? [
              ...result.map((post) => ({ type: 'ForumPost' as const, id: post.id })),
              { type: 'ForumPost' as const, id: 'LIST' },
            ]
          : [{ type: 'ForumPost' as const, id: 'LIST' }],
    }),
    getBookmarkedForumList: builder.query<ForumListItem[], ForumListQueryParams | undefined>({
      query: (params = {}) => ({
        url: '/api/v1/posts/bookmarks',
        params: {
          pageNumber: params.pageNumber ?? 1,
          pageSize: params.pageSize ?? 20,
        },
      }),
      transformResponse: (response: ApiSuccessEnvelope<PostsPayload>) => {
        const payload = response?.data
        const posts = payload?.posts ?? payload?.items ?? []
        return posts.map(toSafeForumListItem)
      },
      providesTags: (result) =>
        result
          ? [
              ...result.map((post) => ({ type: 'ForumPost' as const, id: post.id })),
              { type: 'ForumPost' as const, id: 'BOOKMARKED_LIST' },
            ]
          : [{ type: 'ForumPost' as const, id: 'BOOKMARKED_LIST' }],
    }),
    getForumPostById: builder.query<ForumDetailItem, string>({
      query: (id) => {
        const pid = normalizeForumPostId(id)
        return {
          url: `/api/v1/posts/${pid}`,
        }
      },
      serializeQueryArgs: ({ queryArgs }) => normalizeForumPostId(queryArgs),
      transformResponse: (response: ApiSuccessEnvelope<RawForumPost>, _meta, id) => {
        const payload = response?.data ?? {}
        return toSafeForumDetailItem(payload, id)
      },
      providesTags: (_result, _error, id) => [{ type: 'ForumPost' as const, id: normalizeForumPostId(id) }],
    }),
    getPostComments: builder.query<ForumCommentItem[], { postId: string; pageNumber?: number; pageSize?: number }>({
      query: ({ postId, pageNumber = 1, pageSize = 20 }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/comments/post/${pid}`,
          params: { pageNumber, pageSize },
        }
      },
      transformResponse: (response: ApiSuccessEnvelope<CommentsPayload>, _meta, arg) => {
        const payload = response?.data
        const comments = Array.isArray(payload) ? payload : (payload?.comments ?? payload?.items ?? [])
        const pid = normalizeForumPostId(arg.postId)
        return comments.map((comment, index) => toSafeForumCommentItem(comment, pid, index))
      },
      providesTags: (_result, _error, { postId }) => [
        { type: 'Comment' as const, id: `POST-${normalizeForumPostId(postId)}` },
      ],
    }),
    addComment: builder.mutation<void, AddCommentRequest>({
      query: ({ postId, content, parentCommentId }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/comments/posts/${pid}`,
          method: 'POST',
          body: { content, parentCommentId },
        }
      },
      invalidatesTags: (_result, _error, { postId }) => {
        const pid = normalizeForumPostId(postId)
        return [
          { type: 'Comment' as const, id: `POST-${pid}` },
          { type: 'ForumPost' as const, id: pid },
          { type: 'ForumPost' as const, id: 'LIST' },
        ]
      },
    }),
    votePost: builder.mutation<void, VotePostRequest>({
      query: ({ postId, voteType }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/posts/${pid}/vote`,
          method: 'POST',
          body: { voteType },
        }
      },
      async onQueryStarted({ postId, voteType }, { dispatch, queryFulfilled }) {
        const pid = normalizeForumPostId(postId)
        if (!pid) return
        const patch = dispatch(
          forumListApi.util.updateQueryData('getForumPostById', pid, (draft) => {
            if (voteType === 1) {
              const delta = draft.isUpvoted ? -1 : 1
              draft.voteScore = (draft.voteScore ?? 0) + delta
              draft.isUpvoted = !draft.isUpvoted
            }
          }),
        )
        try {
          await queryFulfilled
        } catch {
          patch.undo()
        }
      },
      invalidatesTags: (_result, _error, { postId }) => {
        const pid = normalizeForumPostId(postId)
        return [
          { type: 'ForumPost' as const, id: pid },
          { type: 'ForumPost' as const, id: 'LIST' },
          { type: 'ForumPost' as const, id: 'BOOKMARKED_LIST' },
        ]
      },
    }),
    voteComment: builder.mutation<void, VoteCommentRequest>({
      query: ({ commentId, voteType }) => ({
        url: `/api/v1/comments/${commentId}/vote`,
        method: 'POST',
        body: { voteType },
      }),
      async onQueryStarted({ commentId, postId, voteType }, { dispatch, queryFulfilled }) {
        const pid = normalizeForumPostId(postId)
        if (!pid) return
        const scoreValueFromVote = (value: 0 | 1 | 2) => {
          if (value === 1) return 1
          if (value === 2) return -1
          return 0
        }
        const patch = dispatch(
          forumListApi.util.updateQueryData(
            'getPostComments',
            { postId: pid, pageNumber: 1, pageSize: 30 },
            (draft) => {
              const c = draft.find((x) => x.id === commentId)
              if (!c) return
              const previousVote = c.currentUserVote ?? 0
              const nextVote: 0 | 1 | 2 = previousVote !== 0 ? 0 : voteType
              const delta = scoreValueFromVote(nextVote) - scoreValueFromVote(previousVote)
              c.voteScore = (c.voteScore ?? 0) + delta
              c.currentUserVote = nextVote
            },
          ),
        )
        try {
          await queryFulfilled
        } catch {
          patch.undo()
        }
      },
      invalidatesTags: (_result, _error, { postId }) => [
        { type: 'Comment' as const, id: `POST-${normalizeForumPostId(postId)}` },
      ],
    }),
    acceptAnswer: builder.mutation<void, AcceptAnswerRequest>({
      query: ({ commentId, postId }) => ({
        url: `/api/v1/comments/${encodeURIComponent(commentId)}/accept`,
        method: 'POST',
        params: { postId },
      }),
      invalidatesTags: (_result, _error, { postId }) => [
        { type: 'Comment' as const, id: `POST-${normalizeForumPostId(postId)}` },
        { type: 'ForumPost' as const, id: normalizeForumPostId(postId) },
      ],
    }),
    pinComment: builder.mutation<void, PinCommentRequest>({
      query: ({ commentId, postId }) => ({
        url: `/api/v1/comments/${encodeURIComponent(commentId)}/pin`,
        method: 'POST',
        params: { postId },
      }),
      invalidatesTags: (_result, _error, { postId }) => [
        { type: 'Comment' as const, id: `POST-${normalizeForumPostId(postId)}` },
      ],
    }),
    bookmarkPost: builder.mutation<void, { postId: string }>({
      query: ({ postId }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/posts/${pid}/bookmark`,
          method: 'POST',
        }
      },
      invalidatesTags: (_result, _error, { postId }) => {
        const pid = normalizeForumPostId(postId)
        return [
          { type: 'ForumPost' as const, id: pid },
          { type: 'ForumPost' as const, id: 'LIST' },
          { type: 'ForumPost' as const, id: 'BOOKMARKED_LIST' },
        ]
      },
    }),
    unbookmarkPost: builder.mutation<void, { postId: string }>({
      query: ({ postId }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/posts/${pid}/bookmark`,
          method: 'DELETE',
        }
      },
      invalidatesTags: (_result, _error, { postId }) => {
        const pid = normalizeForumPostId(postId)
        return [
          { type: 'ForumPost' as const, id: pid },
          { type: 'ForumPost' as const, id: 'LIST' },
        ]
      },
    }),
    reportPost: builder.mutation<void, ReportPostRequest>({
      query: ({ postId, reason, description }) => {
        const pid = normalizeForumPostId(postId)
        return {
          url: `/api/v1/posts/${pid}/report`,
          method: 'POST',
          body: { reason, description },
        }
      },
      invalidatesTags: (_result, _error, { postId }) => {
        const pid = normalizeForumPostId(postId)
        return [
          { type: 'ForumPost' as const, id: pid },
          { type: 'ForumPost' as const, id: 'LIST' },
        ]
      },
    }),
  }),
})

export const {
  useGetForumListQuery,
  useGetBookmarkedForumListQuery,
  useGetForumPostByIdQuery,
  useGetPostCommentsQuery,
  useAddCommentMutation,
  useVotePostMutation,
  useVoteCommentMutation,
  useAcceptAnswerMutation,
  usePinCommentMutation,
  useBookmarkPostMutation,
  useUnbookmarkPostMutation,
  useReportPostMutation,
  useGetForumCategoriesQuery,
  useGetForumThreadChannelsQuery,
  useCreateForumPostMutation,
  useUploadForumAttachmentsMutation,
  usePublishForumPostMutation,
  useGetPopularForumTagsQuery,
  useDeleteCommentMutation,
} = forumListApi
