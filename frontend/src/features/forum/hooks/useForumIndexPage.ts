import { useMemo } from 'react'
import { useGetForumCategoriesQuery, useGetForumListQuery } from '../api/forum.list.api'
import { buildCategoryGroups, getLeafCategories } from '../lib/forumCategoryTree'
import type { ForumListItem } from '../types/forum-list'

export type CategoryLatestPost = {
  post: ForumListItem
  messageEstimate: number
}

function postBelongsToCategory(post: ForumListItem, categoryId: string, categoryName: string): boolean {
  if (post.categoryId) {
    return post.categoryId === categoryId
  }
  return post.category.trim().toLowerCase() === categoryName.trim().toLowerCase()
}

export function useForumIndexPage() {
  const { data: categories = [], isLoading: loadingCategories, isError: categoriesError } =
    useGetForumCategoriesQuery()
  const { data: posts = [], isLoading: loadingPosts, isError: postsError } = useGetForumListQuery({
    pageNumber: 1,
    pageSize: 100,
  })

  const groups = useMemo(() => buildCategoryGroups(categories), [categories])
  const leafCategories = useMemo(() => getLeafCategories(categories), [categories])

  const postCountByCategoryId = useMemo(() => {
    const map = new Map<string, number>()
    for (const category of leafCategories) {
      const count = posts.filter((p) => postBelongsToCategory(p, category.id, category.name)).length
      map.set(category.id, count)
    }
    return map
  }, [leafCategories, posts])

  const latestByCategoryId = useMemo(() => {
    const map = new Map<string, CategoryLatestPost>()
    for (const category of leafCategories) {
      const inCategory = posts
        .filter((p) => postBelongsToCategory(p, category.id, category.name))
        .sort((a, b) => new Date(b.activityAt).getTime() - new Date(a.activityAt).getTime())

      const latest = inCategory[0]
      if (!latest) continue

      const messageEstimate = inCategory.reduce((sum, p) => sum + p.replyCount, 0)
      map.set(category.id, { post: latest, messageEstimate })
    }
    return map
  }, [leafCategories, posts])

  const totals = useMemo(() => {
    const threadCount = [...postCountByCategoryId.values()].reduce((sum, count) => sum + count, 0)
    return { threadCount, categoryCount: leafCategories.length, zoneCount: groups.length }
  }, [leafCategories.length, groups.length, postCountByCategoryId])

  return {
    groups,
    latestByCategoryId,
    postCountByCategoryId,
    totals,
    isLoading: loadingCategories || loadingPosts,
    isError: categoriesError || postsError,
    isEmpty: !loadingCategories && categories.length === 0,
  }
}
