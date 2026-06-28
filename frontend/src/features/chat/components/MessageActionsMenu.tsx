import { useTranslation } from 'react-i18next'

export function MessageActionsMenu({
  open,
  canCopy,
  canReply,
  canEdit,
  canReport,
  canDelete,
  deleteLoading,
  onCopy,
  onReply,
  onEdit,
  onReport,
  onDelete,
}: {
  open: boolean
  canCopy: boolean
  canReply: boolean
  canEdit: boolean
  canReport: boolean
  canDelete: boolean
  deleteLoading?: boolean
  onCopy: () => void
  onReply: () => void
  onEdit: () => void
  onReport: () => void
  onDelete: () => void
}) {
  const { t } = useTranslation()
  if (!open) return null

  return (
    <div className="absolute right-0 z-10 mt-1 min-w-[9rem] rounded-lg border border-slate-200 bg-white py-1 text-left text-sm shadow-lg">
      {canCopy && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-slate-800 hover:bg-slate-50"
          onClick={onCopy}
        >
          {t('chat.message.copy')}
        </button>
      )}
      {canReply && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-slate-800 hover:bg-slate-50"
          onClick={onReply}
        >
          {t('chat.reply.action')}
        </button>
      )}
      {canEdit && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-slate-800 hover:bg-slate-50"
          onClick={onEdit}
        >
          {t('chat.message.edit')}
        </button>
      )}
      {canReport && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-slate-800 hover:bg-slate-50"
          onClick={onReport}
        >
          {t('chat.safety.report')}
        </button>
      )}
      {canDelete && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-red-600 hover:bg-red-50"
          disabled={deleteLoading}
          onClick={onDelete}
        >
          {t('chat.message.unsend')}
        </button>
      )}
    </div>
  )
}
