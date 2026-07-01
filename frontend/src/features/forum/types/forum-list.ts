export type ForumListItem = {
  id: string
  title: string
  category: string
  categorySlug?: string
  categoryId?: string
  threadChannelId?: string
  threadChannelCode?: string
  threadChannelName?: string
  tags: string[]
  replyCount: number
  likeCount: number
  viewCount: number
  bookmarkCount: number
  authorId?: string
  authorName?: string
  authorAvatar?: string
  preview: string
  isPinned: boolean
  isLocked: boolean
  isSolved: boolean
  activityAt: string
}
