import { useState, type FormEvent } from 'react'
import type { AssignBadgeRequest } from '../../types/admin.types'

const BADGE_TYPE_OPTIONS = ['Department', 'Club', 'BoardOfDirectors', 'Faculty', 'Company'] as const
const BADGE_PRESETS: Record<string, Array<{ name: string; description: string }>> = {
  Department: [
    { name: 'Phòng Công tác Sinh viên', description: 'Tài khoản chính thức từ phòng ban nhà trường.' },
    { name: 'Phòng Đào tạo', description: 'Hỗ trợ thông tin lịch học, học vụ và đăng ký tín chỉ.' },
  ],
  Club: [
    { name: 'CLB Công nghệ', description: 'Thành viên ban điều hành CLB Công nghệ.' },
    { name: 'CLB Truyền thông', description: 'Thành viên ban điều hành CLB Truyền thông.' },
  ],
  BoardOfDirectors: [
    { name: 'Ban Giám hiệu', description: 'Tài khoản thuộc Ban Giám hiệu nhà trường.' },
  ],
  Faculty: [
    { name: 'Khoa CNTT', description: 'Giảng viên/điều phối viên chính thức của Khoa CNTT.' },
    { name: 'Khoa Ngoại ngữ', description: 'Giảng viên/điều phối viên chính thức của Khoa Ngoại ngữ.' },
  ],
  Company: [
    { name: 'Đối tác tuyển dụng', description: 'Đơn vị doanh nghiệp đã xác minh từ nhà trường.' },
    { name: 'Nhà tài trợ học bổng', description: 'Đơn vị đồng hành học bổng đã xác minh.' },
  ],
}

type AssignBadgeModalProps = {
  isOpen: boolean
  isSubmitting: boolean
  title: string
  typeLabel: string
  nameLabel: string
  descriptionLabel: string
  cancelLabel: string
  submitLabel: string
  onClose: () => void
  onSubmit: (input: AssignBadgeRequest) => Promise<void>
}

export function AssignBadgeModal({
  isOpen,
  isSubmitting,
  title,
  typeLabel,
  nameLabel,
  descriptionLabel,
  cancelLabel,
  submitLabel,
  onClose,
  onSubmit,
}: AssignBadgeModalProps) {
  const [badgeType, setBadgeType] = useState('')
  const [badgeName, setBadgeName] = useState('')
  const [description, setDescription] = useState('')

  if (!isOpen) return null

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!badgeType.trim() || !badgeName.trim()) return
    await onSubmit({
      badgeType: badgeType.trim(),
      badgeName: badgeName.trim(),
      description: description.trim(),
    })
    setBadgeType('')
    setBadgeName('')
    setDescription('')
  }

  const handleClose = () => {
    setBadgeType('')
    setBadgeName('')
    setDescription('')
    onClose()
  }

  const presetOptions = badgeType ? BADGE_PRESETS[badgeType] ?? [] : []

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form className="w-full max-w-lg rounded-xl bg-white p-4 shadow-lg" onSubmit={handleSubmit}>
        <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
        <label className="mt-4 block text-sm font-medium text-slate-700">
          {typeLabel}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={badgeType}
            onChange={(event) => setBadgeType(event.target.value)}
            disabled={isSubmitting}
          >
            <option value="">Chọn loại huy hiệu</option>
            {BADGE_TYPE_OPTIONS.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </label>
        {presetOptions.length ? (
          <div className="mt-3 rounded-md border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs font-medium text-slate-700">Mẫu huy hiệu gợi ý</p>
            <div className="mt-2 flex flex-wrap gap-2">
              {presetOptions.map((preset) => (
                <button
                  key={preset.name}
                  type="button"
                  className="rounded-md border border-slate-300 bg-white px-2 py-1 text-xs text-slate-700 hover:bg-slate-100"
                  onClick={() => {
                    setBadgeName(preset.name)
                    setDescription(preset.description)
                  }}
                  disabled={isSubmitting}
                >
                  {preset.name}
                </button>
              ))}
            </div>
          </div>
        ) : null}
        <label className="mt-3 block text-sm font-medium text-slate-700">
          {nameLabel}
          <input
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={badgeName}
            onChange={(event) => setBadgeName(event.target.value)}
            disabled={isSubmitting}
          />
        </label>
        <label className="mt-3 block text-sm font-medium text-slate-700">
          {descriptionLabel}
          <textarea
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            disabled={isSubmitting}
            rows={4}
          />
        </label>
        <div className="mt-4 flex justify-end gap-2">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-700 hover:bg-slate-50"
            onClick={handleClose}
            disabled={isSubmitting}
          >
            {cancelLabel}
          </button>
          <button
            type="submit"
            className="rounded-md bg-rose-600 px-3 py-2 text-sm font-medium text-white hover:bg-rose-700 disabled:opacity-60"
            disabled={isSubmitting}
          >
            {submitLabel}
          </button>
        </div>
      </form>
    </div>
  )
}
