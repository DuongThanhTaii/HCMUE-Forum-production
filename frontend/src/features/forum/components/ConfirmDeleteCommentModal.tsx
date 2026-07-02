import { type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'

type ConfirmDeleteCommentModalProps = {
  isOpen: boolean
  isDeleting: boolean
  onClose: () => void
  onConfirm: () => Promise<void>
}

export function ConfirmDeleteCommentModal({
  isOpen,
  isDeleting,
  onClose,
  onConfirm,
}: ConfirmDeleteCommentModalProps) {
  const { t } = useTranslation()

  if (!isOpen) return null

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    await onConfirm()
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form className="w-full max-w-sm rounded-xl bg-white p-6 shadow-lg" onSubmit={handleSubmit}>
        <h3 className="text-lg font-semibold text-slate-900">
          {t('forum.commentSection.confirmDeleteTitle', 'Xác nhận xóa')}
        </h3>
        <p className="mt-2 text-sm text-slate-600">
          {t('forum.commentSection.confirmDelete', 'Bạn có chắc chắn muốn xóa bình luận này? Hành động này không thể hoàn tác.')}
        </p>
        
        <div className="mt-6 flex justify-end gap-3">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 transition-colors"
            onClick={onClose}
            disabled={isDeleting}
          >
            {t('common.cancel', 'Hủy')}
          </button>
          <button
            type="submit"
            className="rounded-md bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 transition-colors disabled:opacity-60"
            disabled={isDeleting}
          >
            {t('common.delete', 'Xóa')}
          </button>
        </div>
      </form>
    </div>
  )
}
