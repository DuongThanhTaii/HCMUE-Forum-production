export type ForumPost = {
  id: string
  title: string
  authorName?: string
  authorId?: string
  createdAt: string
  voteCount?: number
  voteScore?: number
  commentCount: number
}
