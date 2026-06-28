import { useTranslation } from 'react-i18next'
import type { CourseListItem, FacultyListItem } from '@shared/types/learning'

type LearningFiltersRowProps = {
  searchInput: string
  onSearchInputChange: (value: string) => void
  facultyId: string
  onFacultyChange: (value: string) => void
  courseId: string
  onCourseChange: (value: string) => void
  faculties: FacultyListItem[]
  courses: CourseListItem[]
  onClear: () => void
}

export function LearningFiltersRow({
  searchInput,
  onSearchInputChange,
  facultyId,
  onFacultyChange,
  courseId,
  onCourseChange,
  faculties,
  courses,
  onClear,
}: LearningFiltersRowProps) {
  const { t } = useTranslation()

  return (
    <section className="forum-compact-card flex flex-col gap-2 px-3 py-2.5 sm:flex-row sm:flex-wrap sm:items-end">
      <label className="flex min-w-[200px] flex-1 flex-col gap-1">
        <span className="text-[11px] font-medium uppercase tracking-wide text-slate-500">
          {t('learning.filters.search')}
        </span>
        <input
          type="search"
          value={searchInput}
          onChange={(e) => onSearchInputChange(e.target.value)}
          className="rounded-md border border-slate-200 bg-white px-2.5 py-1.5 text-[13px] text-slate-800 outline-none ring-primary focus:ring-2"
          placeholder={t('learning.filters.search')}
          autoComplete="off"
        />
      </label>
      <label className="flex min-w-[160px] flex-col gap-1">
        <span className="text-[11px] font-medium uppercase tracking-wide text-slate-500">
          {t('learning.filters.selectFaculty')}
        </span>
        <select
          value={facultyId}
          onChange={(e) => onFacultyChange(e.target.value)}
          className="rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] text-slate-800 outline-none ring-primary focus:ring-2"
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
          {t('learning.filters.selectCourse')}
        </span>
        <select
          value={courseId}
          onChange={(e) => onCourseChange(e.target.value)}
          className="rounded-md border border-slate-200 bg-white px-2 py-1.5 text-[13px] text-slate-800 outline-none ring-primary focus:ring-2"
        >
          <option value="">{t('learning.filters.allCourses')}</option>
          {courses.map((c) => (
            <option key={c.courseId} value={c.courseId}>
              {c.code} — {c.name}
            </option>
          ))}
        </select>
      </label>
      <button
        type="button"
        onClick={onClear}
        className="rounded-md border border-slate-200 bg-white px-3 py-1.5 text-[13px] font-medium text-slate-600 hover:border-primary hover:text-primary"
      >
        {t('learning.filters.clearFilters')}
      </button>
    </section>
  )
}
