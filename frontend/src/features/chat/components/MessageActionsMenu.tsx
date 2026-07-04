import { useTranslation } from 'react-i18next'

export function MessageActionsMenu({
  open,
  canCopy,
  canReply,
  canEdit,
  canReport,
  canDelete,
  deleteLoading,
  className = 'right-0',
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
  className?: string
  onCopy: () => void
  onReply: () => void
  onEdit: () => void
  onReport: () => void
  onDelete: () => void
}) {
  const { t } = useTranslation()
  if (!open) return null

  return (
    <div className={`absolute z-10 mt-1 min-w-[9rem] rounded-lg border border-border bg-surface py-1 text-left text-sm shadow-lg ${className}`}>
      {canCopy && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-foreground hover:bg-background"
          onClick={onCopy}
        >
          {t('chat.message.copy')}
        </button>
      )}
      {canReply && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-foreground hover:bg-background"
          onClick={onReply}
        >
          {t('chat.reply.action')}
        </button>
      )}
      {canEdit && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-foreground hover:bg-background"
          onClick={onEdit}
        >
          {t('chat.message.edit')}
        </button>
      )}
      {canReport && (
        <button
          type="button"
          className="block w-full px-3 py-1.5 text-left text-foreground hover:bg-background"
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
