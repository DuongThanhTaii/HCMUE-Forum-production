/** REST + hub-aligned identifiers for UI routing (see docs/plans/2026-05-02-chat-channel-rest-notes.md). */
export type ConversationThreadRef = {
  kind: 'conversation'
  conversationId: string
}

export type ChannelThreadRef = {
  kind: 'channel'
  channelId: string
}

export type ChatThreadRef = ConversationThreadRef | ChannelThreadRef

export function threadKey(ref: ChatThreadRef): string {
  return ref.kind === 'conversation'
    ? `conversation:${ref.conversationId}`
    : `channel:${ref.channelId}`
}

export type ConversationDto = {
  id: string
  type: string
  participantIds: string[]
  lastMessageAt: string | null
  createdAt: string
  isArchived: boolean
  /** Group title from server; null for direct. */
  title: string | null
  /** Populated for direct chats: the other participant. */
  directPeerUserId: string | null
  directPeerFullName: string | null
  directPeerEmail: string | null
  isMuted: boolean
  isBlockedWithPeer: boolean
}

export type ChatMessageReportReason =
  | 'Spam'
  | 'Harassment'
  | 'Inappropriate'
  | 'Other'

export type MessageAttachmentDto = {
  fileName: string
  fileUrl: string
  fileSize: number
  mimeType: string
  thumbnailUrl?: string | null
}

export type MessageDto = {
  id: string
  conversationId: string
  senderId: string
  content: string
  type: string
  sentAt: string
  editedAt: string | null
  isDeleted: boolean
  replyToMessageId: string | null
  reactions: Record<string, string[]>
  attachments: MessageAttachmentDto[]
  /** Resolved from identity when available (group / DM). */
  senderDisplayName?: string | null
}

export type ReadReceiptDto = {
  userId: string
  readAt: string
}

export type ReplyTarget = {
  messageId: string
  preview: string
  senderLabel: string
}

export type PagedMessagesDto = {
  items: MessageDto[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type MessageSearchHitDto = {
  messageId: string
  sentAt: string
  snippet: string
  senderId: string
  senderDisplayName: string | null
}

export type PagedMessageSearchDto = {
  items: MessageSearchHitDto[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type ConversationMessageSearchFilter = 'all' | 'text' | 'media' | 'links'

export type ConversationAttachmentKind = 'all' | 'image' | 'file' | 'voice'

export type ConversationAttachmentDto = {
  messageId: string
  sentAt: string
  fileName: string
  fileUrl: string
  mimeType: string
  thumbnailUrl: string | null
  fileSize: number
}

export type PagedConversationAttachmentsDto = {
  items: ConversationAttachmentDto[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type ConversationLinkDto = {
  messageId: string
  sentAt: string
  url: string
  host: string
}

export type PagedConversationLinksDto = {
  items: ConversationLinkDto[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type ChannelDto = {
  id: string
  name: string
  description: string | null
  type: string
  ownerId: string
  memberCount: number
  createdAt: string
  isArchived: boolean
}

export type SendMessagePayload = {
  conversationId: string
  content: string
  replyToMessageId?: string | null
}

export type SendMessageResultDto = {
  messageId: string
  sentAt: string
}

export type UploadFileResultDto = {
  fileName: string
  fileUrl: string
  fileSize: number
  contentType: string
}

export type AttachmentPayload = {
  fileName: string
  fileUrl: string
  fileSize: number
  mimeType: string
  thumbnailUrl?: string | null
}

export type SendWithAttachmentsPayload = {
  conversationId: string
  content?: string | null
  attachments: AttachmentPayload[]
  replyToMessageId?: string | null
}

export type UserListItemDto = {
  id: string
  email: string
  fullName: string
  bio?: string | null
  status: string
  createdAt: string
}

/** Hub: `WebRtcSignalNotification` — signaling only; media is peer-to-peer. */
export type WebRtcSignalPayload = {
  conversationId: string
  fromUserId: string
  fromUserName: string
  kind: string
  payload: string
}

/** Hub payload: aligns with `MessageNotification` (camelCase JSON). */
export type HubMessageNotification = {
  messageId: string
  conversationId: string
  channelId?: string | null
  senderId: string
  senderName: string
  content: string
  messageType: string
  sentAt: string
  replyToMessageId?: string | null
  attachments?: MessageAttachmentDto[]
}
