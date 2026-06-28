import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import { useRegisterCompanyMutation, useUploadCompanyLogoMutation } from '../api/career.api'

export function CareerCompanyRegisterPage() {
  const { t } = useTranslation()
  const userId = useAppSelector(selectUserId)
  const [registerCompany, { isLoading }] = useRegisterCompanyMutation()
  const [uploadCompanyLogo, { isLoading: isUploadingLogo }] = useUploadCompanyLogoMutation()
  const [message, setMessage] = useState<{ type: 'ok' | 'err'; text: string } | null>(null)

  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [address, setAddress] = useState('')
  const [website, setWebsite] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [industry, setIndustry] = useState(0)
  const [size, setSize] = useState(4)
  const [foundedYear, setFoundedYear] = useState<number | ''>('')
  const [linkedIn, setLinkedIn] = useState('')

  async function onLogoFileChange(event: ChangeEvent<HTMLInputElement>) {
    const selectedFile = event.target.files?.[0]
    if (!selectedFile) return

    setMessage(null)
    try {
      const uploaded = await uploadCompanyLogo(selectedFile).unwrap()
      if (uploaded.url) {
        setLogoUrl(uploaded.url)
      } else {
        setMessage({ type: 'err', text: 'Upload logo thất bại, vui lòng thử lại.' })
      }
    } catch {
      setMessage({ type: 'err', text: 'Upload logo thất bại, vui lòng thử lại.' })
    } finally {
      event.target.value = ''
    }
  }

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage(null)
    if (!userId) {
      setMessage({ type: 'err', text: t('auth.pages.companyRegisterRequireLogin') })
      return
    }

    try {
      await registerCompany({
        name: name.trim(),
        description: description.trim(),
        email: email.trim(),
        phone: phone.trim() || undefined,
        address: address.trim() || undefined,
        website: website.trim() || undefined,
        logoUrl: logoUrl.trim() || undefined,
        linkedIn: linkedIn.trim() || undefined,
        industry,
        size,
        registeredBy: userId,
        foundedYear: foundedYear === '' ? undefined : foundedYear,
      }).unwrap()

      setMessage({ type: 'ok', text: `${t('auth.pages.companyRegisterSuccess')} (Đang chờ Admin duyệt)` })
      setName('')
      setDescription('')
      setEmail('')
      setPhone('')
      setAddress('')
      setWebsite('')
      setLogoUrl('')
      setLinkedIn('')
      setFoundedYear('')
    } catch {
      setMessage({ type: 'err', text: t('auth.pages.companyRegisterFailed') })
    }
  }

  return (
    <div className="space-y-4 rounded-xl border border-slate-200 bg-white p-4">
      <header>
        <h2 className="text-lg font-semibold text-slate-900">{t('auth.pages.companyRegisterTitle')}</h2>
        <p className="mt-1 text-sm text-slate-600">{t('auth.pages.companyRegisterDescription')}</p>
      </header>

      {!userId ? (
        <div className="rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
          {t('auth.pages.companyRegisterRequireLogin')}{' '}
          <Link to="/login" className="font-medium underline">
            {t('auth.login')}
          </Link>
        </div>
      ) : null}

      <form className="grid gap-3 md:grid-cols-2" onSubmit={onSubmit}>
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Tên doanh nghiệp" value={name} onChange={(e) => setName(e.target.value)} required />
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Email liên hệ" value={email} onChange={(e) => setEmail(e.target.value)} required />
        <textarea className="md:col-span-2 rounded-md border border-slate-300 px-3 py-2 text-sm" rows={3} placeholder="Mô tả doanh nghiệp" value={description} onChange={(e) => setDescription(e.target.value)} required />
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Số điện thoại" value={phone} onChange={(e) => setPhone(e.target.value)} />
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Địa chỉ" value={address} onChange={(e) => setAddress(e.target.value)} />
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Website" value={website} onChange={(e) => setWebsite(e.target.value)} />
        <div className="space-y-2">
          <input className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Logo URL" value={logoUrl} onChange={(e) => setLogoUrl(e.target.value)} />
          <label className="block text-xs text-slate-500">Hoặc tải logo từ máy (PNG/JPG/WebP)</label>
          <input
            type="file"
            accept="image/*"
            className="block w-full text-xs text-slate-700"
            onChange={onLogoFileChange}
            disabled={isUploadingLogo || !userId}
          />
          {isUploadingLogo ? <p className="text-xs text-slate-500">Đang upload logo...</p> : null}
          {logoUrl ? (
            <div className="flex items-center gap-2">
              <img src={logoUrl} alt="Company logo preview" className="h-10 w-10 rounded border border-slate-200 bg-white object-contain p-1" />
              <span className="text-xs text-emerald-700">Logo đã sẵn sàng</span>
            </div>
          ) : null}
        </div>
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="LinkedIn URL" value={linkedIn} onChange={(e) => setLinkedIn(e.target.value)} />
        <input className="rounded-md border border-slate-300 px-3 py-2 text-sm" type="number" min={1800} max={2100} placeholder="Năm thành lập" value={foundedYear} onChange={(e) => setFoundedYear(e.target.value ? Number(e.target.value) : '')} />
        <select className="rounded-md border border-slate-300 px-3 py-2 text-sm" value={industry} onChange={(e) => setIndustry(Number(e.target.value))}>
          <option value={0}>Technology</option>
          <option value={1}>Finance</option>
          <option value={5}>Manufacturing</option>
          <option value={11}>Consulting</option>
          <option value={15}>Other</option>
        </select>
        <select className="rounded-md border border-slate-300 px-3 py-2 text-sm" value={size} onChange={(e) => setSize(Number(e.target.value))}>
          <option value={0}>Startup</option>
          <option value={1}>Small</option>
          <option value={2}>Medium</option>
          <option value={3}>Large</option>
          <option value={4}>Enterprise</option>
        </select>

        <button
          type="submit"
          disabled={isLoading || isUploadingLogo || !userId}
          className="md:col-span-2 rounded-md bg-primary px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {isLoading ? t('common.loading') : t('auth.pages.companyRegisterSubmit')}
        </button>
      </form>

      {message ? (
        <p className={message.type === 'ok' ? 'text-sm text-emerald-700' : 'text-sm text-rose-700'}>
          {message.text}
        </p>
      ) : null}
    </div>
  )
}
