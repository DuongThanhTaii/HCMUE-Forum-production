import { baseApi } from '@shared/lib/api/baseApi'
import { unwrapApiData, unwrapApiList } from '@features/admin/api/admin.api'
import type {
  ChannelDto,
  ConversationDto,
  MessageAttachmentDto,
  ConversationAttachmentDto,
  ConversationAttachmentKind,
  ConversationLinkDto,
  ConversationMessageSearchFilter,
  MessageDto,
  PagedConversationAttachmentsDto,
  PagedConversationLinksDto,
  MessageSearchHitDto,
  PagedMessageSearchDto,
  PagedMessagesDto,
  ReadReceiptDto,
  SendMessagePayload,
  SendMessageResultDto,
  SendWithAttachmentsPayload,
  UploadFileResultDto,
  UserListItemDto,
} from '../types/chat.types'

const CHAT_LIST = 'LIST' as const

function mapAttachment(raw: Record<string, unknown>): MessageAttachmentDto {
  return {
    fileName: String(raw.fileName ?? ''),
    fileUrl: String(raw.fileUrl ?? ''),
    fileSize: Number(raw.fileSize ?? 0),
    mimeType: String(raw.mimeType ?? ''),
    thumbnailUrl: raw.thumbnailUrl != null ? String(raw.thumbnailUrl) : null,
  }
}

function mapAttachments(raw: unknown): MessageAttachmentDto[] {
  if (!Array.isArray(raw)) return []
  return raw.map((x) => mapAttachment(x as Record<string, unknown>))
}

function mapMessage(raw: Record<string, unknown>): MessageDto {
  const senderDisplayNameRaw = raw.senderDisplayName
  return {
    id: String(raw.id ?? ''),
    conversationId: String(raw.conversationId ?? ''),
    senderId: String(raw.senderId ?? ''),
    content: String(raw.content ?? ''),
    type: String(raw.type ?? 'Text'),
    sentAt: String(raw.sentAt ?? ''),
    editedAt: raw.editedAt != null ? String(raw.editedAt) : null,
    isDeleted: Boolean(raw.isDeleted),
    replyToMessageId: raw.replyToMessageId != null ? String(raw.replyToMessageId) : null,
    reactions:
      raw.reactions && typeof raw.reactions === 'object' && !Array.isArray(raw.reactions)
        ? (raw.reactions as Record<string, string[]>)
        : {},
    attachments: mapAttachments(raw.attachments),
    senderDisplayName:
      senderDisplayNameRaw != null && String(senderDisplayNameRaw).trim().length > 0
        ? String(senderDisplayNameRaw).trim()
        : null,
  }
}

function mapConversation(raw: Record<string, unknown>): ConversationDto {
  const p = raw.participantIds
  const participantIds = Array.isArray(p) ? p.map((x) => String(x)) : []
  return {
    id: String(raw.id ?? ''),
    type: String(raw.type ?? ''),
    participantIds,
    lastMessageAt: raw.lastMessageAt != null ? String(raw.lastMessageAt) : null,
    createdAt: String(raw.createdAt ?? ''),
    isArchived: Boolean(raw.isArchived),
    title: raw.title != null ? String(raw.title) : null,
    directPeerUserId:
      raw.directPeerUserId != null ? String(raw.directPeerUserId) : null,
    directPeerFullName:
      raw.directPeerFullName != null ? String(raw.directPeerFullName) : null,
    directPeerEmail:
      raw.directPeerEmail != null ? String(raw.directPeerEmail) : null,
    isMuted: Boolean(raw.isMuted),
    isBlockedWithPeer: Boolean(raw.isBlockedWithPeer),
  }
}

function mapChannel(raw: Record<string, unknown>): ChannelDto {
  return {
    id: String(raw.id ?? ''),
    name: String(raw.name ?? ''),
    description: raw.description != null ? String(raw.description) : null,
    type: String(raw.type ?? ''),
    ownerId: String(raw.ownerId ?? ''),
    memberCount: Number(raw.memberCount ?? 0),
    createdAt: String(raw.createdAt ?? ''),
    isArchived: Boolean(raw.isArchived),
  }
}

function mapMessageSearchHit(raw: Record<string, unknown>): MessageSearchHitDto {
  const senderDisplayNameRaw = raw.senderDisplayName
  return {
    messageId: String(raw.messageId ?? ''),
    sentAt: String(raw.sentAt ?? ''),
    snippet: String(raw.snippet ?? ''),
    senderId: String(raw.senderId ?? ''),
    senderDisplayName:
      senderDisplayNameRaw != null && String(senderDisplayNameRaw).trim().length > 0
        ? String(senderDisplayNameRaw).trim()
        : null,
  }
}

function mapConversationAttachment(raw: Record<string, unknown>): ConversationAttachmentDto {
  return {
    messageId: String(raw.messageId ?? ''),
    sentAt: String(raw.sentAt ?? ''),
    fileName: String(raw.fileName ?? ''),
    fileUrl: String(raw.fileUrl ?? ''),
    mimeType: String(raw.mimeType ?? ''),
    thumbnailUrl: raw.thumbnailUrl != null ? String(raw.thumbnailUrl) : null,
    fileSize: Number(raw.fileSize ?? raw.fileSizeBytes ?? 0),
  }
}

function mapPagedAttachments(data: unknown): PagedConversationAttachmentsDto {
  const raw = (unwrapApiData<Record<string, unknown>>(data) ?? {}) as Record<string, unknown>
  const itemsRaw = raw.items ?? raw.Items
  const items: ConversationAttachmentDto[] = Array.isArray(itemsRaw)
    ? itemsRaw.map((x) => mapConversationAttachment(x as Record<string, unknown>))
    : []
  return {
    items,
    page: Number(raw.page ?? 1),
    pageSize: Number(raw.pageSize ?? 30),
    totalCount: Number(raw.totalCount ?? 0),
    totalPages: Number(raw.totalPages ?? 0),
  }
}

function mapConversationLink(raw: Record<string, unknown>): ConversationLinkDto {
  return {
    messageId: String(raw.messageId ?? ''),
    sentAt: String(raw.sentAt ?? ''),
    url: String(raw.url ?? ''),
    host: String(raw.host ?? ''),
  }
}

function mapPagedLinks(data: unknown): PagedConversationLinksDto {
  const raw = (unwrapApiData<Record<string, unknown>>(data) ?? {}) as Record<string, unknown>
  const itemsRaw = raw.items
  const items: ConversationLinkDto[] = Array.isArray(itemsRaw)
    ? itemsRaw.map((x) => mapConversationLink(x as Record<string, unknown>))
    : []
  return {
    items,
    page: Number(raw.page ?? 1),
    pageSize: Number(raw.pageSize ?? 30),
    totalCount: Number(raw.totalCount ?? 0),
    totalPages: Number(raw.totalPages ?? 0),
  }
}

function mapPagedMessageSearch(data: unknown): PagedMessageSearchDto {
  const raw = (unwrapApiData<Record<string, unknown>>(data) ?? {}) as Record<string, unknown>
  const itemsRaw = raw.items
  const items: MessageSearchHitDto[] = Array.isArray(itemsRaw)
    ? itemsRaw.map((x) => mapMessageSearchHit(x as Record<string, unknown>))
    : []
  return {
    items,
    page: Number(raw.page ?? 1),
    pageSize: Number(raw.pageSize ?? 20),
    totalCount: Number(raw.totalCount ?? 0),
    totalPages: Number(raw.totalPages ?? 0),
  }
}

function mapPagedMessages(data: unknown): PagedMessagesDto {
  const raw = (unwrapApiData<Record<string, unknown>>(data) ?? {}) as Record<string, unknown>
  const itemsRaw = raw.items
  const items: MessageDto[] = Array.isArray(itemsRaw)
    ? itemsRaw.map((x) => mapMessage(x as Record<string, unknown>))
    : []
  return {
    items,
    page: Number(raw.page ?? 1),
    pageSize: Number(raw.pageSize ?? 50),
    totalCount: Number(raw.totalCount ?? 0),
    totalPages: Number(raw.totalPages ?? 0),
  }
}

function mapUser(raw: Record<string, unknown>): UserListItemDto {
  return {
    id: String(raw.id ?? ''),
    email: String(raw.email ?? ''),
    fullName: String(raw.fullName ?? ''),
    bio: raw.bio != null ? String(raw.bio) : null,
    status: String(raw.status ?? ''),
    createdAt: String(raw.createdAt ?? ''),
  }
}

export const chatApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getConversations: build.query<ConversationDto[], void>({
      query: () => ({ url: '/api/v1/chat/conversations' }),
      transformResponse: (response: unknown) => {
        const inner = unwrapApiList<Record<string, unknown>>(response)
        return inner.map((x) => mapConversation(x))
      },
      providesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
    }),

    getConversationAttachments: build.query<
      PagedConversationAttachmentsDto,
      {
        conversationId: string
        kind?: ConversationAttachmentKind
        page?: number
        pageSize?: number
      }
    >({
      query: ({ conversationId, kind = 'all', page = 1, pageSize = 30 }) => ({
        url: `/api/v1/chat/conversations/${conversationId}/attachments`,
        params: { kind, page, pageSize },
      }),
      transformResponse: (response: unknown) => mapPagedAttachments(response),
    }),

    getConversationLinks: build.query<
      PagedConversationLinksDto,
      { conversationId: string; page?: number; pageSize?: number }
    >({
      query: ({ conversationId, page = 1, pageSize = 30 }) => ({
        url: `/api/v1/chat/conversations/${conversationId}/links`,
        params: { page, pageSize },
      }),
      transformResponse: (response: unknown) => mapPagedLinks(response),
    }),

    searchConversationMessages: build.query<
      PagedMessageSearchDto,
      {
        conversationId: string
        q: string
        filter?: ConversationMessageSearchFilter
        page?: number
        pageSize?: number
      }
    >({
      query: ({ conversationId, q, filter = 'text', page = 1, pageSize = 20 }) => ({
        url: `/api/v1/chat/conversations/${conversationId}/messages/search`,
        params: { q, filter, page, pageSize },
      }),
      transformResponse: (response: unknown) => mapPagedMessageSearch(response),
    }),

    getMessages: build.query<
      PagedMessagesDto,
      { conversationId: string; page?: number; pageSize?: number }
    >({
      query: ({ conversationId, page = 1, pageSize = 50 }) => ({
        url: '/api/v1/chat/messages',
        params: { conversationId, page, pageSize },
      }),
      transformResponse: (response: unknown) => mapPagedMessages(response),
      providesTags: (_result, _err, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
      ],
    }),

    editMessage: build.mutation<
      void,
      { messageId: string; conversationId: string; content: string }
    >({
      query: ({ messageId, content }) => ({
        url: `/api/v1/chat/messages/${messageId}`,
        method: 'PATCH',
        body: { content },
      }),
      invalidatesTags: (_r, _e, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
        { type: 'ChatConversation', id: CHAT_LIST },
      ],
    }),

    deleteMessage: build.mutation<void, { messageId: string; conversationId: string }>({
      query: ({ messageId }) => ({
        url: `/api/v1/chat/messages/${messageId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_r, _e, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
        { type: 'ChatConversation', id: CHAT_LIST },
      ],
    }),

    sendMessage: build.mutation<SendMessageResultDto, SendMessagePayload>({
      query: (body) => ({
        url: '/api/v1/chat/messages',
        method: 'POST',
        body: {
          conversationId: body.conversationId,
          content: body.content,
          replyToMessageId: body.replyToMessageId ?? null,
        },
      }),
      transformResponse: (response: unknown) => {
        const raw = unwrapApiData<Record<string, unknown>>(response) ?? {}
        return {
          messageId: String(raw.messageId ?? ''),
          sentAt: String(raw.sentAt ?? ''),
        }
      },
      invalidatesTags: (_result, _err, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
        { type: 'ChatConversation', id: CHAT_LIST },
      ],
    }),

    uploadChatFile: build.mutation<UploadFileResultDto, FormData>({
      query: (formData) => ({
        url: '/api/v1/chat/messages/upload',
        method: 'POST',
        body: formData,
      }),
      transformResponse: (response: unknown) => {
        const raw = unwrapApiData<Record<string, unknown>>(response) ?? {}
        return {
          fileName: String(raw.fileName ?? ''),
          fileUrl: String(raw.fileUrl ?? ''),
          fileSize: Number(raw.fileSize ?? 0),
          contentType: String(raw.contentType ?? ''),
        }
      },
    }),

    sendMessageWithAttachments: build.mutation<
      SendMessageResultDto,
      SendWithAttachmentsPayload
    >({
      query: (body) => ({
        url: '/api/v1/chat/messages/with-attachments',
        method: 'POST',
        body: {
          conversationId: body.conversationId,
          content: body.content ?? null,
          attachments: body.attachments,
          replyToMessageId: body.replyToMessageId ?? null,
        },
      }),
      transformResponse: (response: unknown) => {
        const raw = unwrapApiData<Record<string, unknown>>(response) ?? {}
        return {
          messageId: String(raw.messageId ?? ''),
          sentAt: String(raw.sentAt ?? ''),
        }
      },
      invalidatesTags: (_result, _err, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
        { type: 'ChatConversation', id: CHAT_LIST },
      ],
    }),

    getPublicChannels: build.query<ChannelDto[], void>({
      query: () => ({ url: '/api/v1/chat/channels/public' }),
      transformResponse: (response: unknown) => {
        const inner = unwrapApiList<Record<string, unknown>>(response)
        return inner.map((x) => mapChannel(x))
      },
      providesTags: [{ type: 'ChatChannel', id: CHAT_LIST }],
    }),

    getMyChannels: build.query<ChannelDto[], void>({
      query: () => ({ url: '/api/v1/chat/channels/my-channels' }),
      transformResponse: (response: unknown) => {
        const inner = unwrapApiList<Record<string, unknown>>(response)
        return inner.map((x) => mapChannel(x))
      },
      providesTags: [{ type: 'ChatChannel', id: CHAT_LIST }],
    }),

    joinChannel: build.mutation<void, string>({
      query: (channelId) => ({
        url: `/api/v1/chat/channels/${channelId}/join`,
        method: 'POST',
      }),
      invalidatesTags: [{ type: 'ChatChannel', id: CHAT_LIST }],
    }),

    leaveChannel: build.mutation<void, string>({
      query: (channelId) => ({
        url: `/api/v1/chat/channels/${channelId}/leave`,
        method: 'POST',
      }),
      invalidatesTags: [{ type: 'ChatChannel', id: CHAT_LIST }],
    }),

    createDirectConversation: build.mutation<{ conversationId: string }, { otherUserId: string }>(
      {
        query: (body) => ({
          url: '/api/v1/chat/conversations/direct',
          method: 'POST',
          body: { otherUserId: body.otherUserId },
        }),
        transformResponse: (response: unknown) => {
          const raw = unwrapApiData<Record<string, unknown>>(response) ?? {}
          return { conversationId: String(raw.conversationId ?? '') }
        },
        invalidatesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
      }
    ),

    createGroupConversation: build.mutation<
      { conversationId: string },
      { title: string; participantIds: string[] }
    >({
      query: (body) => ({
        url: '/api/v1/chat/conversations/group',
        method: 'POST',
        body: {
          title: body.title,
          participantIds: body.participantIds,
        },
      }),
      transformResponse: (response: unknown) => {
        const raw = unwrapApiData<Record<string, unknown>>(response) ?? {}
        return { conversationId: String(raw.conversationId ?? '') }
      },
      invalidatesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
    }),

    /**
     * User directory search for DM picker (`search` + `take` query params).
     * Named differently from admin `getUsers` to avoid RTK Query endpoint collision / override.
     */
    blockUser: build.mutation<void, string>({
      query: (userId) => ({
        url: `/api/v1/users/${userId}/block`,
        method: 'POST',
      }),
      invalidatesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
    }),

    unblockUser: build.mutation<void, string>({
      query: (userId) => ({
        url: `/api/v1/users/${userId}/block`,
        method: 'DELETE',
      }),
      invalidatesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
    }),

    setConversationMute: build.mutation<void, { conversationId: string; muted: boolean }>({
      query: ({ conversationId, muted }) => ({
        url: `/api/v1/chat/conversations/${conversationId}/mute`,
        method: 'POST',
        body: { muted },
      }),
      invalidatesTags: [{ type: 'ChatConversation', id: CHAT_LIST }],
    }),

    reportMessage: build.mutation<
      void,
      { messageId: string; reason: string; description?: string | null }
    >({
      query: ({ messageId, reason, description }) => ({
        url: `/api/v1/chat/messages/${messageId}/report`,
        method: 'POST',
        body: { reason, description: description ?? null },
      }),
    }),

    addMessageReaction: build.mutation<
      void,
      { messageId: string; conversationId: string; emoji: string }
    >({
      query: ({ messageId, emoji }) => ({
        url: `/api/v1/chat/messages/${messageId}/reactions`,
        method: 'POST',
        body: { emoji },
      }),
      invalidatesTags: (_r, _e, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
      ],
    }),

    removeMessageReaction: build.mutation<
      void,
      { messageId: string; conversationId: string; emoji: string }
    >({
      query: ({ messageId, emoji }) => ({
        url: `/api/v1/chat/messages/${messageId}/reactions/${encodeURIComponent(emoji)}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_r, _e, arg) => [
        { type: 'ChatMessage', id: arg.conversationId },
      ],
    }),

    markMessageAsRead: build.mutation<void, { messageId: string; conversationId: string }>({
      query: ({ messageId }) => ({
        url: `/api/v1/chat/messages/${messageId}/read`,
        method: 'POST',
      }),
      // Avoid refetching the whole thread on every read receipt (scroll stability).
    }),

    getMessageReadReceipts: build.query<ReadReceiptDto[], string>({
      query: (messageId) => ({
        url: `/api/v1/chat/messages/${messageId}/read-receipts`,
      }),
      transformResponse: (response: unknown) => {
        const inner = unwrapApiList<Record<string, unknown>>(response)
        return inner.map((raw) => ({
          userId: String(raw.userId ?? ''),
          readAt: String(raw.readAt ?? ''),
        }))
      },
    }),

    searchChatUsers: build.query<UserListItemDto[], { q: string; take?: number }>({
      query: ({ q, take = 24 }) => ({
        url: '/api/v1/users',
        params: { search: q, take },
      }),
      transformResponse: (response: unknown) => {
        const inner = unwrapApiList<Record<string, unknown>>(response)
        return inner.map((x) => mapUser(x))
      },
    }),
  }),
})

export const {
  useGetConversationsQuery,
  useGetMessagesQuery,
  useLazyGetMessagesQuery,
  useSearchConversationMessagesQuery,
  useGetConversationAttachmentsQuery,
  useGetConversationLinksQuery,
  useEditMessageMutation,
  useDeleteMessageMutation,
  useSendMessageMutation,
  useUploadChatFileMutation,
  useSendMessageWithAttachmentsMutation,
  useGetPublicChannelsQuery,
  useGetMyChannelsQuery,
  useJoinChannelMutation,
  useLeaveChannelMutation,
  useCreateDirectConversationMutation,
  useCreateGroupConversationMutation,
  useSearchChatUsersQuery,
  useBlockUserMutation,
  useUnblockUserMutation,
  useSetConversationMuteMutation,
  useReportMessageMutation,
  useAddMessageReactionMutation,
  useRemoveMessageReactionMutation,
  useMarkMessageAsReadMutation,
  useGetMessageReadReceiptsQuery,
  useLazyGetMessageReadReceiptsQuery,
} = chatApi
