import { useMemo, useState, type FormEvent } from 'react'

type AssignRoleModalProps = {
  isOpen: boolean
  isSubmitting: boolean
  title: string
  roleLabel: string
  cancelLabel: string
  submitLabel: string
  roleOptions: Array<{ value: string; label: string }>
  onClose: () => void
  onSubmit: (input: { roleId: string }) => Promise<void>
}

export function AssignRoleModal({
  isOpen,
  isSubmitting,
  title,
  roleLabel,
  cancelLabel,
  submitLabel,
  roleOptions,
  onClose,
  onSubmit,
}: AssignRoleModalProps) {
  const [roleId, setRoleId] = useState('')

  const selectableRoles = useMemo(
    () => roleOptions.filter((option) => option.value !== 'all'),
    [roleOptions],
  )

  if (!isOpen) return null

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!roleId) return
    await onSubmit({ roleId })
    setRoleId('')
  }

  const handleClose = () => {
    setRoleId('')
    onClose()
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form className="w-full max-w-lg rounded-xl bg-white p-4 shadow-lg" onSubmit={handleSubmit}>
        <h3 className="text-lg font-semibold text-slate-900">{title}</h3>
        <label className="mt-4 block text-sm font-medium text-slate-700">
          {roleLabel}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={roleId}
            onChange={(event) => setRoleId(event.target.value)}
            disabled={isSubmitting}
          >
            <option value="" />
            {selectableRoles.map((role) => (
              <option key={role.value} value={role.value}>
                {role.label}
              </option>
            ))}
          </select>
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
            disabled={isSubmitting || !roleId}
          >
            {submitLabel}
          </button>
        </div>
      </form>
    </div>
  )
}
