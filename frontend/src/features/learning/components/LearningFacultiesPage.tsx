import { Link } from 'react-router-dom'
import { useLearningFacultiesPage } from '../hooks/useLearningFacultiesPage'

export function LearningFacultiesPage() {
  const { t, faculties, isLoading, isError, isEmpty } = useLearningFacultiesPage()

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

  if (isEmpty) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-4 text-slate-600">
        {t('learning.messages.noFaculties')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header>
        <h1 className="text-lg font-semibold text-slate-900">{t('learning.facultiesPage.title')}</h1>
        <p className="mt-1 text-sm text-slate-600">{t('learning.facultiesPage.subtitle')}</p>
        <p className="mt-2 text-[13px]">
          <Link to="/learning/documents" className="text-primary hover:underline">
            {t('learning.documentList')}
          </Link>
        </p>
      </header>
      <ul className="grid gap-3 sm:grid-cols-2">
        {faculties.map((f) => (
          <li key={f.facultyId} className="forum-compact-card flex flex-col gap-2 p-3">
            <div>
              <h2 className="text-base font-semibold text-slate-900">{f.name}</h2>
              <p className="text-[12px] text-slate-500">
                {t('learning.documentDetailPage.code')}: {f.code}
              </p>
              <p className="mt-1 line-clamp-3 text-[13px] text-slate-600">{f.description}</p>
            </div>
            <div className="flex flex-wrap gap-2 text-[13px]">
              <span className="text-slate-600">
                {t('learning.facultiesPage.courses')}: <strong>{f.courseCount}</strong>
              </span>
            </div>
            <div className="flex flex-wrap gap-2 pt-1">
              <Link
                to={`/learning/courses?facultyId=${f.facultyId}`}
                className="rounded-md border border-primary px-2.5 py-1 text-[13px] font-medium text-primary hover:bg-primary/5"
              >
                {t('learning.facultiesPage.viewCourses')}
              </Link>
              <Link
                to={`/learning/documents?facultyId=${f.facultyId}`}
                className="rounded-md border border-slate-200 px-2.5 py-1 text-[13px] font-medium text-slate-700 hover:border-primary hover:text-primary"
              >
                {t('learning.facultiesPage.viewDocuments')}
              </Link>
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}
