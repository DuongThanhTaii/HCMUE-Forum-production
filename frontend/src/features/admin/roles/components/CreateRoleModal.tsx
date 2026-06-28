import { useState, type FormEvent } from 'react'

type CreateRoleModalProps = {
  isOpen: boolean
  isSubmitting: boolean
  title: string
  nameLabel: string
  descriptionLabel: string
  cancelLabel: string
  submitLabel: string
  onClose: () => void
  onSubmit: (input: { name: string; description: string }) => Promise<void>
}

export function CreateRoleModal({
  isOpen,
  isSubmitting,
  title,
  nameLabel,
  descriptionLabel,
  cancelLabel,
  submitLabel,
  onClose,
  onSubmit,
}: CreateRoleModalProps) {
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  if (!isOpen) return null

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!name.trim()) return

    await onSubmit({ name: name.trim(), description: description.trim() })
    setName('')
    setDescription('')
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form className="w-full max-w-lg rounded-xl bg-white p-4 shadow-lg" onSubmit={handleSubmit}>
        <h3 className="text-lg font-semibold text-slate-900">{title}</h3>

        <label className="mt-4 block text-sm font-medium text-slate-700">
          {nameLabel}
          <input
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={name}
            onChange={(event) => setName(event.target.value)}
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
            onClick={onClose}
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
