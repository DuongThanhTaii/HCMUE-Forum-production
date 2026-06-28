import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import {
  useGetForumCategoriesQuery,
  useGetForumListQuery,
} from '../api/forum.list.api'
import type { ForumFilterTab } from '../components/ForumFiltersRow'

export function useForumListPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const categoryId = searchParams.get('category')?.trim() || undefined

  const { data: categories = [] } = useGetForumCategoriesQuery()
  const activeCategory = categoryId
    ? categories.find((c) => c.id === categoryId)
    : undefined

  const { data, isLoading, isError } = useGetForumListQuery({
    pageNumber: 1,
    pageSize: categoryId ? 50 : 20,
    categoryId,
  })
  const [activeTab, setActiveTab] = useState<ForumFilterTab>('latest')

  const filteredItems = useMemo(() => {
    const source = [...(data ?? [])]

    if (activeTab === 'tag') {
      return source
        .filter((item) => item.tags.length > 0)
        .sort((a, b) => new Date(b.activityAt).getTime() - new Date(a.activityAt).getTime())
    }

    if (activeTab === 'category') {
      return source.sort((a, b) => {
        const categoryCompare = a.category.localeCompare(b.category)
        if (categoryCompare !== 0) {
          return categoryCompare
        }
        return new Date(b.activityAt).getTime() - new Date(a.activityAt).getTime()
      })
    }

    return source.sort((a, b) => {
      if (activeTab === 'hot') {
        return b.replyCount - a.replyCount
      }
      return new Date(b.activityAt).getTime() - new Date(a.activityAt).getTime()
    })
  }, [data, activeTab])

  return {
    t,
    activeTab,
    setActiveTab,
    filteredItems,
    isLoading,
    isError,
    isEmpty: !isLoading && !isError && !data?.length,
    categoryId,
    activeCategory,
  }
}
