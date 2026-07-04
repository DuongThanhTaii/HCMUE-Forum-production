import { useMemo, useState } from 'react'

import { useTranslation } from 'react-i18next'

import { X } from 'lucide-react'

import {

  useGetConversationAttachmentsQuery,

  useGetConversationLinksQuery,

} from '../api/chat.api'

import {

  isFileAttachment,

  isImageAttachment,

  isVoiceAttachment,

} from '../lib/attachmentKind'

import {

  conversationAttachmentsFromMessages,

  mergeConversationAttachments,

} from '../lib/conversationAttachmentsFromMessages'

import { resolveChatAssetUrl } from '../lib/mediaUrl'

import type {
  ConversationAttachmentDto,
  ConversationLinkDto,
  MessageDto,
} from '../types/chat.types'

import { SharedFilesList } from './SharedFilesList'

import { SharedLinksList } from './SharedLinksList'

import { SharedMediaGrid } from './SharedMediaGrid'



type InfoTab = 'media' | 'files' | 'links'



export function ConversationInfoDrawer({

  conversationId,

  title,

  peerUserId,

  isMuted = false,

  isBlockedWithPeer = false,

  open,

  onClose,

  onJumpToMessage,

  threadMessages = [],

}: {

  conversationId: string

  title?: React.ReactNode | null

  peerUserId?: string | null

  isMuted?: boolean

  isBlockedWithPeer?: boolean

  open: boolean

  onClose: () => void

  onJumpToMessage?: (messageId: string) => void | Promise<void>

  /** Tin đang hiển thị trong thread (gồm trang cũ đã cuộn tải) — dùng bổ sung API media. */
  threadMessages?: MessageDto[]

}) {

  const { t } = useTranslation()

  const [tab, setTab] = useState<InfoTab>('media')



  const {

    data: allAttachments,

    isFetching: attachmentsLoading,

    isError: attachmentsError,

  } = useGetConversationAttachmentsQuery(

    { conversationId, kind: 'all', page: 1, pageSize: 100 },

    { skip: !open },

  )



  const mergedAttachments = useMemo(() => {

    const fromApi = allAttachments?.items ?? []

    const fromThread = conversationAttachmentsFromMessages(threadMessages)

    return mergeConversationAttachments(fromApi, fromThread)

  }, [allAttachments?.items, threadMessages])



  const imageItems = useMemo(

    () => mergedAttachments.filter((a) => isImageAttachment(a.mimeType, a.fileName)),

    [mergedAttachments],

  )



  const fileItems = useMemo(

    () =>

      mergedAttachments

        .filter((a) => isFileAttachment(a.mimeType, a.fileName) || isVoiceAttachment(a.mimeType, a.fileName))

        .sort((a, b) => new Date(b.sentAt).getTime() - new Date(a.sentAt).getTime()),

    [mergedAttachments],

  )



  const { data: links, isFetching: linksLoading } = useGetConversationLinksQuery(

    { conversationId, page: 1, pageSize: 40 },

    { skip: !open || tab !== 'links' },

  )



  if (!open) {

    return null

  }



  const tabs: { id: InfoTab; label: string }[] = [

    { id: 'media', label: t('chat.info.media') },

    { id: 'files', label: t('chat.info.files') },

    { id: 'links', label: t('chat.info.links') },

  ]



  const openImage = (item: ConversationAttachmentDto) => {

    window.open(resolveChatAssetUrl(item.fileUrl), '_blank', 'noopener,noreferrer')

  }



  const handleLinkSelect = (item: ConversationLinkDto) => {

    if (onJumpToMessage) {

      void onJumpToMessage(item.messageId)

      onClose()

    }

  }



  return (

    <div

      className="absolute inset-0 z-30 flex flex-col bg-surface"

      role="dialog"

      aria-label={t('chat.info.title')}

    >

      <div className="flex items-start gap-2 border-b border-border px-3 py-3">
        <div className="min-w-0 flex-1">
          <h3 className="text-sm font-semibold text-foreground">{t('chat.info.title')}</h3>
          {title ? <p className="truncate text-xs text-muted">{title}</p> : null}
        </div>
        <button
          type="button"
          onClick={onClose}
          className="cursor-pointer rounded-lg p-1.5 text-muted hover:bg-background"
          aria-label={t('common.cancel')}
        >
          <X className="h-4 w-4" />
        </button>
      </div>

      {/* Theme Selector */}
      <div className="border-b border-border px-3 py-3">
        <h4 className="mb-2 text-xs font-semibold text-foreground">Giao diện (Theme)</h4>
        <div className="flex gap-2">
          {(['light', 'dark', 'ocean', 'sunset', 'forest'] as const).map((tName) => {
            // Colors for preview
            const bgMap: Record<string, string> = {
              light: '#f8fafc',
              dark: '#0f172a',
              ocean: '#0ea5e9',
              sunset: '#f97316',
              forest: '#16a34a',
            }
            return (
              <button
                key={tName}
                onClick={() => {
                  document.documentElement.classList.remove('theme-light', 'theme-dark', 'theme-ocean', 'theme-sunset', 'theme-forest')
                  document.documentElement.classList.add(`theme-${tName}`)
                  localStorage.setItem('app-theme', tName)
                }}
                className="h-6 w-6 cursor-pointer rounded-full border border-border shadow-sm transition-transform hover:scale-110"
                style={{ backgroundColor: bgMap[tName] }}
                title={tName.charAt(0).toUpperCase() + tName.slice(1)}
              />
            )
          })}
        </div>
      </div>

      <div className="space-y-2 border-b border-border px-3 py-3 text-xs text-muted">

        {peerUserId ? (

          <p>

            <span className="font-medium text-foreground">{t('chat.info.peer')}: </span>

            {title ?? peerUserId}

          </p>

        ) : null}

        <p>

          {isMuted ? t('chat.info.mutedOn') : t('chat.info.mutedOff')}

          {isBlockedWithPeer ? ` · ${t('chat.safety.blockedBanner')}` : ''}

        </p>

        <p className="text-muted">{t('chat.info.shortcutsHint')}</p>

      </div>



      <div className="flex gap-1 border-b border-border px-2 py-2">

        {tabs.map((item) => (

          <button

            key={item.id}

            type="button"

            onClick={() => setTab(item.id)}

            className={`cursor-pointer rounded-lg px-3 py-1.5 text-xs font-medium ${

              tab === item.id

                ? 'bg-primary text-primary-foreground'

                : 'text-muted hover:bg-background'

            }`}

          >

            {item.label}

          </button>

        ))}

      </div>



      <div className="min-h-0 flex-1 overflow-y-auto px-3 py-3">

        {tab === 'media' && (

          <>

            {attachmentsLoading ? (

              <p className="text-center text-sm text-muted">{t('common.loading')}</p>

            ) : attachmentsError && imageItems.length === 0 ? (

              <p className="text-center text-sm text-red-600">{t('chat.info.loadError')}</p>

            ) : !imageItems.length ? (

              <p className="text-center text-sm text-muted">{t('chat.info.emptyMedia')}</p>

            ) : (

              <SharedMediaGrid items={imageItems} onSelect={openImage} />

            )}

          </>

        )}



        {tab === 'files' && (

          <>

            {attachmentsLoading ? (

              <p className="text-center text-sm text-muted">{t('common.loading')}</p>

            ) : !fileItems.length ? (

              <p className="text-center text-sm text-muted">{t('chat.info.emptyFiles')}</p>

            ) : (

              <SharedFilesList items={fileItems} />

            )}

          </>

        )}



        {tab === 'links' && (

          <>

            {linksLoading ? (

              <p className="text-center text-sm text-muted">{t('common.loading')}</p>

            ) : !(links?.items.length) ? (

              <p className="text-center text-sm text-muted">{t('chat.info.emptyLinks')}</p>

            ) : (

              <SharedLinksList

                items={links.items}

                onSelect={onJumpToMessage ? handleLinkSelect : undefined}

              />

            )}

          </>

        )}

      </div>

    </div>

  )

}


