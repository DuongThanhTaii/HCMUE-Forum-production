import { useMemo } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { useGetJobByIdQuery } from '../api/career.api'
import { parseCareerDescription } from '../lib/jobDescription'

type RouteState = {
  companyName?: string
}

export function CareerJobDetailPage() {
  const { id = '' } = useParams()
  const location = useLocation()
  const routeState = (location.state as RouteState | null) ?? null
  const { data, isLoading, isError } = useGetJobByIdQuery(id, { skip: !id })

  const sections = useMemo(() => parseCareerDescription(data?.description), [data?.description])

  if (isLoading) {
    return <div className="rounded-xl border border-slate-200 bg-white p-4 text-slate-600">Đang tải JD...</div>
  }

  if (isError || !data) {
    return <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">Không tải được chi tiết job.</div>
  }

  return (
    <div className="space-y-4">
      <Link to="/career/jobs" className="text-sm font-medium text-primary hover:underline">
        ← Quay lại danh sách việc làm
      </Link>

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        <h1 className="text-xl font-semibold text-slate-900">{data.title}</h1>
        <p className="mt-1 text-sm text-slate-600">
          {(routeState?.companyName ?? 'Doanh nghiệp')} · {data.city} · {data.isRemote ? 'Làm việc từ xa' : 'Làm việc tại văn phòng'}
        </p>
      </section>

      {sections.images.length > 0 ? (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-900">Job Visuals</h2>
          <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
            {sections.images.map((url) => (
              <a key={url} href={url} target="_blank" rel="noreferrer" className="block">
                <img
                  src={url}
                  alt="Job visual"
                  className="h-auto w-full rounded border border-slate-200 bg-white object-contain"
                />
              </a>
            ))}
          </div>
        </section>
      ) : null}

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        <h2 className="text-base font-semibold text-slate-900">Mô tả công việc</h2>
        {sections.overview.length > 0 ? (
          <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-slate-700">
            {sections.overview.map((line, idx) => (
              <li key={`${idx}-${line.slice(0, 16)}`}>{line}</li>
            ))}
          </ul>
        ) : (
          <p className="mt-2 text-sm text-slate-600">Chưa có mô tả chi tiết.</p>
        )}
      </section>

      {sections.profile.length > 0 ? (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-900">Hồ sơ của bạn</h2>
          <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-slate-700">
            {sections.profile.map((line, idx) => (
              <li key={`${idx}-${line.slice(0, 16)}`}>{line}</li>
            ))}
          </ul>
        </section>
      ) : null}

      {sections.benefits.length > 0 ? (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-900">Quyền lợi</h2>
          <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-slate-700">
            {sections.benefits.map((line, idx) => (
              <li key={`${idx}-${line.slice(0, 16)}`}>{line}</li>
            ))}
          </ul>
        </section>
      ) : null}

      {sections.additional.length > 0 ? (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h2 className="text-base font-semibold text-slate-900">Liên hệ & thông tin bổ sung</h2>
          <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-slate-700">
            {sections.additional.map((line, idx) => (
              <li key={`${idx}-${line.slice(0, 16)}`}>{line}</li>
            ))}
          </ul>
        </section>
      ) : null}
    </div>
  )
}
