import { useMemo, useState, type FormEvent } from 'react'
import {
  useCreateJobPostingMutation,
  useGetCompanyApplicationsQuery,
  useGetJobsQuery,
  useGetMyCompaniesQuery,
  usePublishJobPostingMutation,
  useUploadCompanyLogoMutation,
} from '../api/career.api'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import { composeCareerDescription, parseCareerDescription } from '../lib/jobDescription'

export function CareerCompanyDashboardPage() {
  const userId = useAppSelector(selectUserId)
  const { data: myCompanies = [] } = useGetMyCompaniesQuery()
  const activeCompany = myCompanies[0]
  const { data: jobs = [], isLoading } = useGetJobsQuery(
    activeCompany ? { page: 1, pageSize: 100, companyId: activeCompany.id } : undefined,
    { skip: !activeCompany },
  )
  const { data: applications } = useGetCompanyApplicationsQuery(activeCompany?.id ?? '', {
    skip: !activeCompany,
  })
  const [createJobPosting, { isLoading: isCreating }] = useCreateJobPostingMutation()
  const [publishJobPosting, { isLoading: isPublishing }] = usePublishJobPostingMutation()
  const [uploadImage, { isLoading: isUploadingImage }] = useUploadCompanyLogoMutation()

  const [title, setTitle] = useState('')
  const [jobDescription, setJobDescription] = useState('')
  const [candidateProfile, setCandidateProfile] = useState('')
  const [benefits, setBenefits] = useState('')
  const [additionalInfo, setAdditionalInfo] = useState('')
  const [imageUrls, setImageUrls] = useState<string[]>([])
  const [city, setCity] = useState('Ho Chi Minh City')
  const [isRemote, setIsRemote] = useState(true)
  const [message, setMessage] = useState<string | null>(null)

  const myJobs = useMemo(() => jobs, [jobs])

  const draftJobs = myJobs.filter((j) => (j as { status?: string }).status?.toLowerCase() === 'draft')
  const publishedJobs = myJobs.filter((j) => (j as { status?: string }).status?.toLowerCase() === 'published')

  async function onCreateJob(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!activeCompany || !userId) return
    setMessage(null)
    const mergedDescription = composeCareerDescription({
      overview: jobDescription,
      profile: candidateProfile,
      benefits,
      additional: additionalInfo,
      imageUrls,
    })

    try {
      await createJobPosting({
        title: title.trim(),
        description: mergedDescription,
        companyId: activeCompany.id,
        postedBy: userId,
        jobType: 0,
        experienceLevel: 0,
        city: city.trim(),
        isRemote,
      }).unwrap()
      setTitle('')
      setJobDescription('')
      setCandidateProfile('')
      setBenefits('')
      setAdditionalInfo('')
      setImageUrls([])
      setMessage('Đã tạo job ở trạng thái Draft.')
    } catch {
      setMessage('Tạo job thất bại. Kiểm tra quyền recruiter/company đã duyệt.')
    }
  }

  async function onPublish(jobId: string) {
    try {
      await publishJobPosting(jobId).unwrap()
      setMessage('Đã publish job.')
    } catch {
      setMessage('Publish thất bại.')
    }
  }

  async function onUploadImages(files: FileList | null) {
    if (!files || files.length === 0) return
    setMessage(null)
    try {
      const uploads = await Promise.all(Array.from(files).map((file) => uploadImage(file).unwrap()))
      const urls = uploads.map((item) => item.url).filter((url) => Boolean(url?.trim()))
      setImageUrls((prev) => Array.from(new Set([...prev, ...urls])))
      setMessage('Đã upload ảnh thành công.')
    } catch {
      setMessage('Upload ảnh thất bại. Vui lòng thử lại.')
    }
  }

  if (!activeCompany) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-4 text-slate-700">
        Bạn chưa có công ty để quản lý. Vào trang đăng ký doanh nghiệp trước.
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-slate-200 bg-white p-4">
        <h2 className="text-lg font-semibold text-slate-900">Doanh nghiệp: {activeCompany.name}</h2>
        <p className="mt-1 text-sm text-slate-600">Trạng thái: {activeCompany.status}</p>
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-4">
        <h3 className="text-base font-semibold text-slate-900">Tạo Job</h3>
        <form className="mt-3 grid gap-3" onSubmit={onCreateJob}>
          <input className="rounded border border-slate-300 px-3 py-2 text-sm" placeholder="Tiêu đề" value={title} onChange={(e) => setTitle(e.target.value)} required />
          <div className="rounded-lg border border-slate-200 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">Bố cục nội dung JD</p>
            <p className="mt-1 text-xs text-slate-500">Mỗi dòng là một ý. Hệ thống sẽ render theo từng section ở trang chi tiết job.</p>
            <div className="mt-3 grid gap-3">
              <textarea
                className="rounded border border-slate-300 px-3 py-2 text-sm"
                rows={5}
                placeholder="Job Overview (trách nhiệm chính)"
                value={jobDescription}
                onChange={(e) => setJobDescription(e.target.value)}
                required
              />
              <textarea
                className="rounded border border-slate-300 px-3 py-2 text-sm"
                rows={4}
                placeholder="Candidate Profile (yêu cầu ứng viên)"
                value={candidateProfile}
                onChange={(e) => setCandidateProfile(e.target.value)}
                required
              />
              <textarea
                className="rounded border border-slate-300 px-3 py-2 text-sm"
                rows={4}
                placeholder="Benefits (quyền lợi)"
                value={benefits}
                onChange={(e) => setBenefits(e.target.value)}
              />
              <textarea
                className="rounded border border-slate-300 px-3 py-2 text-sm"
                rows={4}
                placeholder="Contact & additional info"
                value={additionalInfo}
                onChange={(e) => setAdditionalInfo(e.target.value)}
              />
            </div>
          </div>
          <div className="rounded-lg border border-slate-200 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">Ảnh cho bài đăng tuyển</p>
            <input
              type="file"
              multiple
              accept="image/*"
              onChange={(e) => void onUploadImages(e.target.files)}
              className="mt-2 w-full rounded border border-slate-300 px-2 py-1.5 text-xs file:mr-2 file:cursor-pointer file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1"
            />
            {isUploadingImage ? <p className="mt-2 text-xs text-slate-500">Đang upload ảnh...</p> : null}
            {imageUrls.length > 0 ? (
              <div className="mt-3 grid grid-cols-2 gap-2 md:grid-cols-4">
                {imageUrls.map((url) => (
                  <div key={url} className="relative rounded border border-slate-200 p-1">
                    <img src={url} alt="Job visual" className="h-auto max-h-44 w-full rounded bg-white object-contain" />
                    <button
                      type="button"
                      onClick={() => setImageUrls((prev) => prev.filter((item) => item !== url))}
                      className="absolute right-1 top-1 rounded bg-black/70 px-1.5 py-0.5 text-[10px] text-white"
                    >
                      X
                    </button>
                  </div>
                ))}
              </div>
            ) : null}
          </div>
          <input className="rounded border border-slate-300 px-3 py-2 text-sm" placeholder="Thành phố" value={city} onChange={(e) => setCity(e.target.value)} />
          <label className="inline-flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" checked={isRemote} onChange={(e) => setIsRemote(e.target.checked)} />
            Làm việc từ xa
          </label>
          <button type="submit" disabled={isCreating} className="w-fit rounded bg-primary px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
            {isCreating ? 'Đang tạo...' : 'Tạo Job'}
          </button>
        </form>
        {message ? <p className="mt-2 text-sm text-slate-600">{message}</p> : null}
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-4">
        <h3 className="text-base font-semibold text-slate-900">Draft</h3>
        {isLoading ? <p className="mt-2 text-sm text-slate-600">Đang tải...</p> : null}
        <div className="mt-3 space-y-3">
          {draftJobs.map((j) => (
            <div key={j.id} className="rounded border border-slate-200 p-3">
              <p className="font-medium text-slate-900">{j.title}</p>
              <p className="text-sm text-slate-600">
                {(parseCareerDescription(j.description ?? '').overview[0] ?? 'Chưa có mô tả').slice(0, 180)}
              </p>
              <button
                className="mt-2 rounded border border-slate-300 px-3 py-1 text-xs font-medium"
                disabled={isPublishing}
                onClick={() => onPublish(j.id)}
              >
                Publish
              </button>
            </div>
          ))}
          {draftJobs.length === 0 ? <p className="text-sm text-slate-600">Chưa có job draft.</p> : null}
        </div>
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-4">
        <h3 className="text-base font-semibold text-slate-900">Published</h3>
        <div className="mt-3 space-y-3">
          {publishedJobs.map((j) => (
            <div key={j.id} className="rounded border border-slate-200 p-3">
              <p className="font-medium text-slate-900">{j.title}</p>
              <p className="text-sm text-slate-600">
                {(parseCareerDescription(j.description ?? '').overview[0] ?? 'Chưa có mô tả').slice(0, 180)}
              </p>
            </div>
          ))}
          {publishedJobs.length === 0 ? <p className="text-sm text-slate-600">Chưa có job đã publish.</p> : null}
        </div>
      </section>

      <section className="rounded-xl border border-slate-200 bg-white p-4">
        <h3 className="text-base font-semibold text-slate-900">Applicants</h3>
        <div className="mt-3 space-y-3">
          {(applications?.applications ?? []).map((a) => (
            <div key={a.applicationId} className="rounded border border-slate-200 p-3">
              <p className="font-medium text-slate-900">{a.jobTitle}</p>
              <p className="text-sm text-slate-600">Ứng viên: {a.applicantName ?? a.applicantId}</p>
              <p className="text-xs text-slate-500">Trạng thái: {a.status}</p>
            </div>
          ))}
          {(applications?.applications ?? []).length === 0 ? <p className="text-sm text-slate-600">Chưa có ứng viên.</p> : null}
        </div>
      </section>
    </div>
  )
}
