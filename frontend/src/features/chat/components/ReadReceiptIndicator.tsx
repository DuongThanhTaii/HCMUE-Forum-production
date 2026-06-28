import { useTranslation } from 'react-i18next'
import { useGetMessageReadReceiptsQuery } from '../api/chat.api'
import { formatReadReceiptLabel } from '../lib/formatReadReceiptLabel'

export function ReadReceiptIndicator({
  messageId,
  currentUserId,
  peerUserId,
  enabled,
}: {
  messageId: string
  currentUserId: string | null
  peerUserId: string | null
  enabled: boolean
}) {
  const { t } = useTranslation()
  const { data: receipts } = useGetMessageReadReceiptsQuery(messageId, {
    skip: !enabled,
  })

  const labelKey = formatReadReceiptLabel(receipts ?? [], currentUserId, peerUserId)
  if (!labelKey) return null

  const text =
    labelKey === 'seenMany'
      ? t('chat.read.seenByMany')
      : t('chat.read.seen')

  return (
    <span className="text-[10px] italic text-indigo-200" title={text}>
      {text}
    </span>
  )
}
