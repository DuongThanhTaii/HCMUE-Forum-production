import { startTransition, useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useGetCoursesQuery, useGetCourseSemestersQuery, useGetFacultiesQuery } from '../api/learning.api'

const COURSES_PAGE_SIZE = 12

/** Body hook — mount inside a parent keyed by faculty URL param so semester state resets when faculty changes. */
export function useLearningCoursesPageBody() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const facultyFromUrl = searchParams.get('facultyId') ?? ''
  const [semester, setSemester] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [page, setPage] = useState(1)

  useEffect(() => {
    const id = window.setTimeout(() => setDebouncedSearch(searchInput.trim()), 400)
    return () => window.clearTimeout(id)
  }, [searchInput])

  useEffect(() => {
    startTransition(() => setPage(1))
  }, [facultyFromUrl, semester, debouncedSearch])

  const { data: faculties = [], isLoading: loadingFaculties } = useGetFacultiesQuery()

  const { data: semesterList = [], isFetching: loadingSemesters } = useGetCourseSemestersQuery(
    facultyFromUrl ? { facultyId: facultyFromUrl } : undefined,
    { skip: !facultyFromUrl },
  )

  const semesterOptions = useMemo(() => [...semesterList].sort((a, b) => a.localeCompare(b, 'vi')), [semesterList])

  const queryArgs = useMemo(
    () => ({
      page,
      pageSize: COURSES_PAGE_SIZE,
      ...(facultyFromUrl ? { facultyId: facultyFromUrl } : {}),
      ...(semester ? { semester } : {}),
      ...(debouncedSearch ? { searchTerm: debouncedSearch } : {}),
    }),
    [facultyFromUrl, semester, debouncedSearch, page],
  )

  const { data: coursesPage, isLoading: loadingCourses, isError } = useGetCoursesQuery(queryArgs)

  const courses = coursesPage?.items ?? []
  const totalPages = coursesPage?.totalPages ?? 0
  const totalCount = coursesPage?.totalCount ?? 0

  const setFacultyFilter = useCallback(
    (facultyId: string) => {
      setSearchParams(
        (prev) => {
          const n = new URLSearchParams(prev)
          if (facultyId) n.set('facultyId', facultyId)
          else n.delete('facultyId')
          return n
        },
        { replace: true },
      )
    },
    [setSearchParams],
  )

  const loading = loadingFaculties || loadingCourses || (!!facultyFromUrl && loadingSemesters)

  return {
    t,
    faculties,
    facultyFromUrl,
    setFacultyFilter,
    semester,
    setSemester,
    semesterOptions,
    searchInput,
    setSearchInput,
    courses,
    page,
    setPage,
    totalPages,
    totalCount,
    pageSize: COURSES_PAGE_SIZE,
    isLoading: loading,
    isError,
    isEmpty: !loading && !isError && courses.length === 0,
  }
}
