import { baseApi } from '@shared/lib/api/baseApi'

type ApiEnvelope<T> = {
  success?: boolean
  message?: string
  data?: T
}

export type UEBotExchangeTokenResponse = {
  syncAccessToken: string
  syncExpiresAt?: string | null
  syncUser?: {
    id: string
    email: string
    name?: string | null
  } | null
  syncApiBaseUrl: string
}

export type SummarizePostResult = {
  postId: string
  summary: string
  keyPoints: string[]
  commentCount: number
  generatedAt: string
}

export type RelatedPostsResult = {
  query: string
  items: Array<{
    id: string
    title: string
    slug: string
    searchRank: number
    reason: string
    citationUrl: string
  }>
}

export type DraftReplyResult = {
  postId: string
  intent: string
  tone: string
  draftMarkdown: string
  generatedAt: string
}

export type ModerationHintResult = {
  postId: string
  isSafe: boolean
  requiresReview: boolean
  isBlocked: boolean
  recommendation: string
  reason: string
  violations: Array<{
    type: string
    severity: number
    confidence: number
    description: string
  }>
  generatedAt: string
}

export type SuggestTitleTagsResult = {
  suggestedTitle: string
  suggestedTags: string[]
  rationale: string
  generatedAt: string
}

export type RewriteContentResult = {
  style: string
  rewrittenContent: string
  generatedAt: string
}

export const assistantApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    exchangeUEBotToken: builder.mutation<UEBotExchangeTokenResponse, void>({
      query: () => ({
        url: '/api/v1/integrations/uebot/exchange-token',
        method: 'POST',
      }),
      transformResponse: (response: ApiEnvelope<UEBotExchangeTokenResponse>) => {
        if (!response?.data?.syncAccessToken) {
          throw new Error('Invalid exchange-token response from server.')
        }
        return response.data
      },
    }),
    summarizePost: builder.mutation<SummarizePostResult, { postId: string; length?: string }>({
      query: ({ postId, length }) => ({
        url: '/api/v1/assistant/tools/summarize-post',
        method: 'POST',
        body: { postId, length },
      }),
      transformResponse: (response: ApiEnvelope<SummarizePostResult>) => {
        if (!response?.data?.summary) {
          throw new Error('Invalid summarize-post response.')
        }
        return response.data
      },
    }),
    relatedPosts: builder.mutation<RelatedPostsResult, { postId?: string; query?: string; limit?: number }>({
      query: ({ postId, query, limit = 5 }) => ({
        url: '/api/v1/assistant/tools/related-posts',
        method: 'POST',
        body: { postId, query, limit },
      }),
      transformResponse: (response: ApiEnvelope<RelatedPostsResult>) => {
        if (!response?.data) {
          throw new Error('Invalid related-posts response.')
        }
        return response.data
      },
    }),
    draftReply: builder.mutation<DraftReplyResult, { postId: string; intent?: string; tone?: string }>({
      query: ({ postId, intent = 'answer', tone = 'friendly' }) => ({
        url: '/api/v1/assistant/tools/draft-reply',
        method: 'POST',
        body: { postId, intent, tone },
      }),
      transformResponse: (response: ApiEnvelope<DraftReplyResult>) => {
        if (!response?.data?.draftMarkdown) {
          throw new Error('Invalid draft-reply response.')
        }
        return response.data
      },
    }),
    moderationHint: builder.mutation<ModerationHintResult, { postId: string }>({
      query: ({ postId }) => ({
        url: '/api/v1/assistant/tools/moderation-hint',
        method: 'POST',
        body: { postId },
      }),
      transformResponse: (response: ApiEnvelope<ModerationHintResult>) => {
        if (!response?.data) {
          throw new Error('Invalid moderation-hint response.')
        }
        return response.data
      },
    }),
    suggestTitleTags: builder.mutation<
      SuggestTitleTagsResult,
      { title: string; content: string; maxTags?: number }
    >({
      query: ({ title, content, maxTags = 5 }) => ({
        url: '/api/v1/assistant/tools/suggest-title-tags',
        method: 'POST',
        body: { title, content, maxTags },
      }),
      transformResponse: (response: ApiEnvelope<SuggestTitleTagsResult>) => {
        if (!response?.data?.suggestedTitle) {
          throw new Error('Invalid suggest-title-tags response.')
        }
        return response.data
      },
    }),
    rewriteContent: builder.mutation<
      RewriteContentResult,
      { title: string; content: string; style?: string }
    >({
      query: ({ title, content, style = 'clear' }) => ({
        url: '/api/v1/assistant/tools/rewrite-content',
        method: 'POST',
        body: { title, content, style },
      }),
      transformResponse: (response: ApiEnvelope<RewriteContentResult>) => {
        if (!response?.data?.rewrittenContent) {
          throw new Error('Invalid rewrite-content response.')
        }
        return response.data
      },
    }),
  }),
})

export const {
  useExchangeUEBotTokenMutation,
  useSummarizePostMutation,
  useRelatedPostsMutation,
  useDraftReplyMutation,
  useModerationHintMutation,
  useSuggestTitleTagsMutation,
  useRewriteContentMutation,
} = assistantApi
