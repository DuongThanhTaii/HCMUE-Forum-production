export type ForumListItem = {
  id: string
  title: string
  category: string
  categoryId?: string
  threadChannelId?: string
  threadChannelCode?: string
  threadChannelName?: string
  tags: string[]
  replyCount: number
  activityAt: string
}
