import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useGetCoursesQuery, useGetDocumentsQuery, useGetFacultiesQuery } from '../api/learning.api'
import {
  documentStatusBadgeClass,
  getDocumentStatusLabel,
  getDocumentStatusTone,
} from '../lib/documentStatus'

const PAGE_SIZE = 20

function formatDate(value: string | undefined, locale: string) {
  if (!value) return '-'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '-' : date.toLocaleDateString(locale)
}

function formatUploaderRef(uploaderId: string | undefined): string {
  if (!uploaderId) return ''
  return uploaderId.length <= 12 ? uploaderId : `${uploaderId.slice(0, 8)}…`
}

export function useLearningDocumentsPage() {
  const { i18n, t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const facultyId = searchParams.get('facultyId') ?? ''
  const courseId = searchParams.get('courseId') ?? ''

  const [searchInput, setSearchInput] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [page, setPage] = useState(1)

  useEffect(() => {
    const id = window.setTimeout(() => {
      const next = searchInput.trim()
      setDebouncedSearch((prev) => {
        if (prev !== next) {
          queueMicrotask(() => setPage(1))
        }
        return next
      })
    }, 400)
    return () => window.clearTimeout(id)
  }, [searchInput])

  const setFacultyId = useCallback(
    (value: string) => {
      setPage(1)
      setSearchParams(
        (prev) => {
          const n = new URLSearchParams(prev)
          if (value) n.set('facultyId', value)
          else n.delete('facultyId')
          n.delete('courseId')
          return n
        },
        { replace: true },
      )
    },
    [setSearchParams],
  )

  const setCourseId = useCallback(
    (value: string) => {
      setPage(1)
      setSearchParams(
        (prev) => {
          const n = new URLSearchParams(prev)
          if (value) n.set('courseId', value)
          else n.delete('courseId')
          return n
        },
        { replace: true },
      )
    },
    [setSearchParams],
  )

  const clearFilters = useCallback(() => {
    setSearchInput('')
    setDebouncedSearch('')
    setPage(1)
    setSearchParams({}, { replace: true })
  }, [setSearchParams])

  const { data: faculties = [], isLoading: loadingFaculties } = useGetFacultiesQuery()
  const { data: coursesPaged, isLoading: loadingCourses } = useGetCoursesQuery(
    facultyId ? { facultyId, page: 1, pageSize: 200 } : undefined,
  )
  const courses = useMemo(() => coursesPaged?.items ?? [], [coursesPaged])

  useEffect(() => {
    if (!courseId) return
    if (loadingCourses) return
    if (courses.some((c) => c.courseId === courseId)) return
    setSearchParams(
      (prev) => {
        const n = new URLSearchParams(prev)
        n.delete('courseId')
        return n
      },
      { replace: true },
    )
  }, [courseId, courses, loadingCourses, setSearchParams])

  const { data, isLoading, isError, isFetching } = useGetDocumentsQuery({
    pageNumber: page,
    pageSize: PAGE_SIZE,
    ...(debouncedSearch ? { searchTerm: debouncedSearch } : {}),
    ...(facultyId ? { facultyId } : {}),
    ...(courseId ? { courseId } : {}),
  })

  const locale = i18n.resolvedLanguage === 'vi' ? 'vi-VN' : 'en-US'

  const documents = useMemo(
    () =>
      (data?.documents ?? []).map((doc) => {
        const status = doc.status ?? ''
        const tone = getDocumentStatusTone(status)
        return {
          ...doc,
          displayUploader:
            doc.uploaderName ??
            (doc.uploaderId ? formatUploaderRef(doc.uploaderId) : t('learning.documentCard.unknownUploader')),
          displayDate: formatDate(doc.createdAt, locale),
          displayDescription: doc.description ?? t('learning.documentCard.noDescription'),
          displayDownloads: Number(doc.downloadCount ?? doc.totalDownloads ?? 0),
          displayRating: Number(doc.averageRating ?? 0),
          displayStatusLabel: status ? getDocumentStatusLabel(status, t) : '',
          displayStatusTone: tone,
          displayStatusClass: documentStatusBadgeClass(tone),
        }
      }),
    [data?.documents, locale, t],
  )

  const totalCount = data?.totalCount ?? 0
  const maxPage = useMemo(() => {
    const tp = data?.totalPages ?? 0
    const tc = data?.totalCount ?? 0
    if (tp > 0) return tp
    if (tc > 0) return Math.max(1, Math.ceil(tc / PAGE_SIZE))
    return 0
  }, [data?.totalCount, data?.totalPages])

  const loading = isLoading || loadingFaculties || loadingCourses

  const goPrev = useCallback(() => setPage((p) => Math.max(1, p - 1)), [])
  const goNext = useCallback(() => {
    setPage((p) => {
      if (!maxPage || maxPage <= 0) return p
      return Math.min(maxPage, p + 1)
    })
  }, [maxPage])

  return {
    t,
    searchInput,
    setSearchInput,
    facultyId,
    setFacultyId,
    courseId,
    setCourseId,
    faculties,
    courses,
    clearFilters,
    documents,
    page,
    pageSize: PAGE_SIZE,
    totalCount,
    totalPages: maxPage,
    goPrev,
    goNext,
    canPrev: page > 1,
    canNext: maxPage > 0 && page < maxPage,
    isLoading: loading,
    isFetching,
    isError,
    isEmpty: !loading && !isError && documents.length === 0,
  }
}
