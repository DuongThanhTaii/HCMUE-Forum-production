import { useCallback, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import {
  useDownloadDocumentMutation,
  useGetDocumentByIdQuery,
  useRateDocumentMutation,
} from '../api/learning.api'

function formatBytes(bytes: number, locale: string) {
  if (!Number.isFinite(bytes) || bytes <= 0) return '—'
  const units = ['B', 'KB', 'MB', 'GB']
  let n = bytes
  let i = 0
  while (n >= 1024 && i < units.length - 1) {
    n /= 1024
    i += 1
  }
  return `${n.toLocaleString(locale, { maximumFractionDigits: 1 })} ${units[i]}`
}

export function useLearningDocumentDetailPage() {
  const { t, i18n } = useTranslation()
  const { id = '' } = useParams<{ id: string }>()
  const userId = useAppSelector(selectUserId)
  const locale = i18n.resolvedLanguage === 'vi' ? 'vi-VN' : 'en-US'

  const { data: doc, isLoading, isError } = useGetDocumentByIdQuery(id, { skip: !id })

  const [rating, setRating] = useState(5)
  const [rateMsg, setRateMsg] = useState<{ kind: 'ok' | 'err'; text: string } | null>(null)
  const [downloadMsg, setDownloadMsg] = useState<{ kind: 'ok' | 'err'; text: string } | null>(null)

  const [rateDocument, { isLoading: ratingSending }] = useRateDocumentMutation()
  const [downloadDocument, { isLoading: downloadSending }] = useDownloadDocumentMutation()

  const formatDate = useCallback(
    (value: string | null | undefined) => {
      if (!value) return '—'
      const d = new Date(value)
      return Number.isNaN(d.getTime()) ? '—' : d.toLocaleDateString(locale)
    },
    [locale],
  )

  const onSubmitRating = useCallback(async () => {
    setRateMsg(null)
    if (!userId || !id) {
      setRateMsg({ kind: 'err', text: t('learning.messages.validationError') })
      return
    }
    try {
      await rateDocument({ documentId: id, userId, rating }).unwrap()
      setRateMsg({ kind: 'ok', text: t('learning.messages.rateSuccess') })
    } catch {
      setRateMsg({ kind: 'err', text: t('learning.messages.rateError') })
    }
  }, [userId, id, rating, rateDocument, t])

  const onDownload = useCallback(async () => {
    setDownloadMsg(null)
    if (!userId || !id) {
      setDownloadMsg({ kind: 'err', text: t('learning.messages.validationError') })
      return
    }
    try {
      await downloadDocument({ documentId: id, userId }).unwrap()
      const fileUrl = doc?.filePath?.trim()
      if (fileUrl) {
        window.open(fileUrl, '_blank', 'noopener,noreferrer')
      }
      setDownloadMsg({ kind: 'ok', text: t('learning.messages.downloadTracked') })
    } catch {
      setDownloadMsg({ kind: 'err', text: t('learning.messages.downloadError') })
    }
  }, [userId, id, downloadDocument, doc?.filePath, t])

  const onShareFacebook = useCallback(() => {
    const directUrl = `${window.location.origin}/learning/documents/${id}`
    const quote = `${doc?.title ?? 'Learning document'}\n${(doc?.description ?? '').slice(0, 180)}`
    const fbShareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(directUrl)}&quote=${encodeURIComponent(quote)}`
    window.open(fbShareUrl, '_blank', 'noopener,noreferrer,width=700,height=560')
  }, [id, doc?.title, doc?.description])

  const fileSizeLabel = doc ? formatBytes(doc.fileSize, locale) : '—'

  return {
    t,
    id,
    doc,
    isLoading,
    isError,
    userId,
    rating,
    setRating,
    ratingSending,
    downloadSending,
    rateMsg,
    downloadMsg,
    formatDate,
    fileSizeLabel,
    onSubmitRating,
    onDownload,
    onShareFacebook,
  }
}
