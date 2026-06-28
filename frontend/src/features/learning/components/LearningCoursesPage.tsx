import { Link, useSearchParams } from 'react-router-dom'
import { useLearningCoursesPageBody } from '../hooks/useLearningCoursesPage'

function LearningCoursesPageInner() {
  const {
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
    pageSize,
    isLoading,
    isError,
    isEmpty,
  } = useLearningCoursesPageBody()

  if (isLoading) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-4">{t('common.loading')}</div>
    )
  }

  if (isError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">
        {t('learning.messages.loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header>
        <h1 className="text-lg font-semibold text-slate-900">{t('learning.coursesPage.title')}</h1>
        <p className="mt-1 text-sm text-slate-600">{t('learning.coursesPage.subtitle')}</p>
        <p className="mt-2 text-[13px]">
          <Link to="/learning/faculties" className="text-primary hover:underline">
            {t('learning.facultyList')}
          </Link>
          {' · '}
          <Link to="/learning/documents" className="text-primary hover:underline">
            {t('learning.documentList')}
          </Link>
        </p>
      </header>

      <section className="forum-compact-card flex flex-col gap-2 px-3 py-2.5 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="flex min-w-[180px] flex-col gap-1">
          <span className="text-[11px] font-medium uppercase tracking-wide text-slate-500">
            {t('learning.filters.selectFaculty')}
          </span>
          <select
            value={facultyFromUrl}
            onChange={(e) => setFacultyFilter(e.target.value)}
            className="rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] outline-none ring-primary focus:ring-2"
          >
            <option value="">{t('learning.filters.allFaculties')}</option>
            {faculties.map((f) => (
              <option key={f.facultyId} value={f.facultyId}>
                {f.name}
              </option>
            ))}
          </select>
        </label>
        <label className="flex min-w-[160px] flex-col gap-1">
          <span className="text-[11px] font-medium uppercase tracking-wide text-slate-500">
            {t('learning.filters.semester')}
          </span>
          <select
            value={semester}
            onChange={(e) => setSemester(e.target.value)}
            className="rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] outline-none ring-primary focus:ring-2"
          >
            <option value="">{t('learning.filters.allSemesters')}</option>
            {semesterOptions.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>
        <label className="flex min-w-[200px] flex-1 flex-col gap-1">
          <span className="text-[11px] font-medium uppercase tracking-wide text-slate-500">
            {t('learning.filters.searchCourse')}
          </span>
          <input
            type="search"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] outline-none ring-primary focus:ring-2"
          />
        </label>
      </section>

      {isEmpty ? (
        <div className="rounded-xl border border-slate-200 bg-white p-4 text-slate-600">
          {t('learning.messages.noCourses')}
        </div>
      ) : (
        <>
          <ul className="grid gap-3 sm:grid-cols-2">
            {courses.map((c) => (
              <li key={c.courseId} className="forum-compact-card flex flex-col gap-2 p-3">
                <div>
                  <h2 className="text-base font-semibold text-slate-900">{c.name}</h2>
                  <p className="text-[12px] text-slate-500">
                    {c.code} · {c.semester}
                  </p>
                  <p className="mt-1 line-clamp-2 text-[13px] text-slate-600">{c.description}</p>
                </div>
                <div className="text-[13px] text-slate-600">
                  {t('learning.coursesPage.credits')} <strong>{c.credits}</strong> · {c.documentCount}{' '}
                  {t('learning.coursesPage.documentsCount')}
                </div>
                <div>
                  <Link
                    to={`/learning/documents?courseId=${c.courseId}`}
                    className="inline-block rounded-md border border-primary px-2.5 py-1 text-[13px] font-medium text-primary hover:bg-primary/5"
                  >
                    {t('learning.coursesPage.viewDocuments')}
                  </Link>
                </div>
              </li>
            ))}
          </ul>
          {totalPages > 1 && (
            <div className="flex flex-wrap items-center justify-between gap-2 border-t border-slate-100 pt-4">
              <p className="text-[13px] text-slate-600">
                {t('learning.coursesPage.showingRange', {
                  from: totalCount === 0 ? 0 : (page - 1) * pageSize + 1,
                  to: Math.min(page * pageSize, totalCount),
                  total: totalCount,
                })}
              </p>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  className="rounded-md border border-slate-200 bg-white px-3 py-1.5 text-[13px] font-medium text-slate-800 disabled:opacity-40"
                >
                  {t('learning.pagination.previous')}
                </button>
                <span className="text-[13px] text-slate-600">
                  {t('learning.pagination.page')} {page} / {totalPages}
                </span>
                <button
                  type="button"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => p + 1)}
                  className="rounded-md border border-slate-200 bg-white px-3 py-1.5 text-[13px] font-medium text-slate-800 disabled:opacity-40"
                >
                  {t('learning.pagination.next')}
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}

/** Remount inner content when faculty filter changes so semester input/query resets without effects. */
export function LearningCoursesPage() {
  const [searchParams] = useSearchParams()
  const facultyKey = searchParams.get('facultyId') ?? '__all__'
  return <LearningCoursesPageInner key={facultyKey} />
}
