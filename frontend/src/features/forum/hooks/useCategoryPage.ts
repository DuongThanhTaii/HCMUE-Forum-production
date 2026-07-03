import { useState, useMemo } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import {
  useGetForumCategoriesQuery,
  useGetForumListQuery,
} from '../api/forum.list.api'

export function useCategoryPage() {
  const { categorySlug } = useParams<{ categorySlug: string }>()
  const [searchParams] = useSearchParams()
  
  const [searchTerm, setSearchTerm] = useState(searchParams.get('q') || searchParams.get('search') || '')
  const [sortBy, setSortBy] = useState<number>(0) // 0 = Newest, 1 = Trending, 2 = Recently Active, 3 = Most Viewed, 4 = Most Liked
  const [pageNumber, setPageNumber] = useState(1)
  
  // Filters
  const [isPinned, setIsPinned] = useState<boolean | undefined>(undefined)
  const [isSolved, setIsSolved] = useState<boolean | undefined>(undefined)
  const [isUnanswered, setIsUnanswered] = useState<boolean | undefined>(undefined)
  
  // Find category ID from slug
  const { data: categories = [], isLoading: isLoadingCategories } = useGetForumCategoriesQuery()
  const activeCategory = useMemo(() => {
    if (categorySlug === 'all') return { id: '', name: 'All Discussions', slug: 'all', description: 'Browse all discussions across all categories.' } as any
    return categories.find(c => c.slug === categorySlug || c.id === categorySlug)
  }, [categories, categorySlug])

  const categoryId = categorySlug === 'all' ? undefined : activeCategory?.id

  // Fetch threads for the active category
  const { data = [], isLoading: isLoadingThreads, isError, isFetching } = useGetForumListQuery({
    pageNumber,
    pageSize: 10,
    categoryId,
    searchTerm: searchTerm || undefined,
    sortBy,
    isPinned,
    isSolved,
    isUnanswered,
  }, {
    skip: categorySlug !== 'all' && !categoryId && !isLoadingCategories, // Skip if specific category not found
  })

  const isLoading = isLoadingCategories || (isLoadingThreads && pageNumber === 1)
  const isEmpty = !isLoading && !isError && data.length === 0

  const loadMore = () => {
    if (!isFetching) {
      setPageNumber(prev => prev + 1)
    }
  }

  // Reset page when filters change
  useMemo(() => {
    setPageNumber(1)
  }, [searchTerm, sortBy, isPinned, isSolved, isUnanswered, categoryId])

  return {
    activeCategory,
    threads: data,
    isLoading,
    isFetching,
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
    loadMore,
  }
}
