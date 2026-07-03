import { Link } from 'react-router-dom'
import { ChevronLeft, PenSquare, Search, Filter, Loader2 } from 'lucide-react'
import { useEffect, useRef } from 'react'
import { useCategoryPage } from '../hooks/useCategoryPage'
import { ThreadCard } from './ThreadCard'

export function CategoryPage() {
  const {
    activeCategory,
    threads,
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
    loadMore,
    isFetching,
  } = useCategoryPage()

  const observerTarget = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && !isFetching && !isLoading && !isEmpty) {
          loadMore()
        }
      },
      { threshold: 0.1 }
    )

    if (observerTarget.current) {
      observer.observe(observerTarget.current)
    }

    return () => observer.disconnect()
  }, [isFetching, isLoading, isEmpty, loadMore])

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value)
  }

  return (
    <div className="space-y-6 max-w-5xl mx-auto px-4 py-6">
      {/* Breadcrumb & Header */}
      <div className="flex flex-col md:flex-row md:items-start justify-between gap-4">
        <div>
          <Link
            to="/explore"
            className="inline-flex items-center gap-1 text-[13px] font-medium text-slate-500 hover:text-primary mb-2 transition-colors"
          >
            <ChevronLeft className="h-4 w-4" />
            Back to Explore
          </Link>
          <h1 className="text-2xl font-bold text-slate-900">
            {activeCategory ? activeCategory.name : 'Discussions'}
          </h1>
          {activeCategory?.description && (
            <p className="mt-1 text-sm text-slate-500">{activeCategory.description}</p>
          )}
        </div>
        <Link
          to="/forum/new"
          className="inline-flex shrink-0 items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-white shadow-sm transition-all hover:bg-primary/90 hover:shadow"
        >
          <PenSquare className="h-4 w-4" />
          New Discussion
        </Link>
      </div>

      {/* Controls: Search, Sort, Filter */}
      <div className="flex flex-col sm:flex-row items-center gap-3 bg-white p-3 rounded-xl border border-slate-200 shadow-sm">
        <div className="relative flex-1 w-full">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search discussions..."
            value={searchTerm}
            onChange={handleSearch}
            className="w-full pl-9 pr-4 py-2 bg-slate-50 border-none rounded-lg text-sm focus:ring-2 focus:ring-primary/20 transition-all outline-none"
          />
        </div>
        <div className="flex items-center gap-2 w-full sm:w-auto">
          <select 
            value={sortBy} 
            onChange={(e) => setSortBy(Number(e.target.value))}
            className="flex-1 sm:flex-none bg-slate-50 border-none rounded-lg px-3 py-2 text-sm text-slate-700 font-medium focus:ring-2 focus:ring-primary/20 outline-none cursor-pointer"
          >
            <option value={0}>Newest</option>
            <option value={1}>Trending</option>
            <option value={2}>Recently Active</option>
            <option value={3}>Most Viewed</option>
            <option value={4}>Most Liked</option>
          </select>

          {/* Simple Dropdown for filters (To be replaced with a proper DropdownMenu component in the future if available) */}
          <div className="relative group">
            <button className="flex items-center justify-center gap-2 bg-slate-50 hover:bg-slate-100 border-none rounded-lg px-3 py-2 text-sm text-slate-700 font-medium transition-colors cursor-pointer">
              <Filter className="h-4 w-4" />
              <span className="hidden sm:inline">Filter</span>
            </button>
            <div className="absolute right-0 top-full mt-2 w-48 bg-white rounded-xl shadow-lg border border-slate-100 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-20 overflow-hidden">
              <div className="p-2 space-y-1">
                <label className="flex items-center gap-2 p-2 hover:bg-slate-50 rounded-lg cursor-pointer">
                  <input type="checkbox" checked={isPinned || false} onChange={(e) => setIsPinned(e.target.checked ? true : undefined)} className="rounded border-slate-300 text-primary focus:ring-primary" />
                  <span className="text-sm font-medium text-slate-700">Pinned</span>
                </label>
                <label className="flex items-center gap-2 p-2 hover:bg-slate-50 rounded-lg cursor-pointer">
                  <input type="checkbox" checked={isSolved || false} onChange={(e) => setIsSolved(e.target.checked ? true : undefined)} className="rounded border-slate-300 text-primary focus:ring-primary" />
                  <span className="text-sm font-medium text-slate-700">Solved</span>
                </label>
                <label className="flex items-center gap-2 p-2 hover:bg-slate-50 rounded-lg cursor-pointer">
                  <input type="checkbox" checked={isUnanswered || false} onChange={(e) => setIsUnanswered(e.target.checked ? true : undefined)} className="rounded border-slate-300 text-primary focus:ring-primary" />
                  <span className="text-sm font-medium text-slate-700">Unanswered</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* State Rendering */}
      {isLoading ? (
        <div className="space-y-3">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="animate-pulse bg-white p-4 rounded-xl border border-slate-100 flex flex-col gap-3">
              <div className="h-5 bg-slate-100 rounded w-2/3"></div>
              <div className="h-4 bg-slate-50 rounded w-full"></div>
              <div className="flex gap-2 mt-2">
                <div className="h-4 bg-slate-100 rounded w-16"></div>
                <div className="h-4 bg-slate-100 rounded w-16"></div>
              </div>
            </div>
          ))}
        </div>
      ) : isError ? (
        <div className="flex flex-col items-center justify-center p-12 bg-rose-50 rounded-2xl border border-rose-100 text-center">
          <div className="w-12 h-12 rounded-full bg-rose-100 flex items-center justify-center mb-3">
            <span className="text-rose-600 font-bold text-xl">!</span>
          </div>
          <h3 className="text-lg font-bold text-rose-900 mb-1">Failed to load discussions</h3>
          <p className="text-rose-600 text-sm max-w-md">There was a network or server error while fetching the threads. Please try again later.</p>
          <button onClick={() => window.location.reload()} className="mt-4 px-4 py-2 bg-white border border-rose-200 text-rose-700 rounded-lg text-sm font-semibold hover:bg-rose-50 transition-colors">
            Retry
          </button>
        </div>
      ) : isEmpty ? (
        <div className="flex flex-col items-center justify-center p-12 bg-slate-50 rounded-2xl border border-slate-100 text-center">
          <div className="w-16 h-16 bg-white shadow-sm border border-slate-100 rounded-2xl flex items-center justify-center mb-4 text-slate-400">
            <Search className="w-8 h-8" />
          </div>
          <h3 className="text-lg font-bold text-slate-900 mb-1">No discussions found</h3>
          <p className="text-slate-500 text-sm max-w-sm">We couldn't find any threads matching your current filters. Try adjusting your search or create a new discussion.</p>
          <Link to="/forum/new" className="mt-6 px-4 py-2 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary/90 transition-colors">
            Create Discussion
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {threads.map((thread) => (
            <ThreadCard key={thread.id} thread={thread} />
          ))}
          
          {/* Intersection Observer Target */}
          <div ref={observerTarget} className="h-10 w-full flex items-center justify-center">
            {isFetching && !isLoading && (
              <Loader2 className="w-6 h-6 text-primary animate-spin" />
            )}
          </div>
        </div>
      )}
    </div>
  )
}
