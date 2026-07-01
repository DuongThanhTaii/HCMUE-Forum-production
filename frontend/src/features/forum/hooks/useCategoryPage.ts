import { useState, useMemo } from 'react'
import { useParams } from 'react-router-dom'
import {
  useGetForumCategoriesQuery,
  useGetForumListQuery,
} from '../api/forum.list.api'

export function useCategoryPage() {
  const { categorySlug } = useParams<{ categorySlug: string }>()
  
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState<number>(0) // 0 = Newest, 1 = Trending, 2 = Recently Active, 3 = Most Viewed, 4 = Most Liked
  
  // Filters
  const [isPinned, setIsPinned] = useState<boolean | undefined>(undefined)
  const [isSolved, setIsSolved] = useState<boolean | undefined>(undefined)
  const [isUnanswered, setIsUnanswered] = useState<boolean | undefined>(undefined)
  
  // Find category ID from slug
  const { data: categories = [], isLoading: isLoadingCategories } = useGetForumCategoriesQuery()
  const activeCategory = useMemo(() => {
    return categories.find(c => c.slug === categorySlug || c.id === categorySlug)
  }, [categories, categorySlug])

  const categoryId = activeCategory?.id

  // Fetch threads for the active category
  const { data = [], isLoading: isLoadingThreads, isError } = useGetForumListQuery({
    pageNumber: 1,
    pageSize: 50,
    categoryId,
    searchTerm: searchTerm || undefined,
    sortBy,
    isPinned,
    isSolved,
    isUnanswered,
  }, {
    skip: !categoryId && !isLoadingCategories, // Skip if category not found after loading
  })

  const isLoading = isLoadingCategories || isLoadingThreads
  const isEmpty = !isLoading && !isError && data.length === 0

  return {
    activeCategory,
    threads: data,
    isLoading,
    isError,
    isEmpty,
    searchTerm,
    setSearchTerm,
    sortBy,
    setSortBy,
    isPinned,
    setIsPinned,
    isSolved,
    setIsSolved,
    isUnanswered,
    setIsUnanswered,
  }
}
