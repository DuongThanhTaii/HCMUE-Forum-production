import { useTranslation } from 'react-i18next'
import { Phone, Video } from 'lucide-react'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import { useChatContext } from '../context/ChatContext'
import { useCallContext } from '../context/CallContext'
import type { ChatThreadRef, ConversationDto } from '../types/chat.types'

type Props = {
  threadRef: ChatThreadRef
  conversation: ConversationDto | null
}

export function ChatCallBar({ threadRef, conversation }: Props) {
  const { t } = useTranslation()
  const { hubStatus } = useChatContext()
  const currentUserId = useAppSelector(selectUserId)
  const call = useCallContext()

  const isConversation = threadRef.kind === 'conversation'
  const convId = isConversation ? threadRef.conversationId : ''
  const remoteUserId =
    conversation?.directPeerUserId ??
    (conversation?.participantIds.find((id) => id !== currentUserId) ?? null)

  const isPair = (conversation?.participantIds.length ?? 0) === 2
  const canCall = isConversation && Boolean(conversation && isPair && remoteUserId)

  // Channel threads: render nothing
  if (!isConversation) return null

  const hubReady = hubStatus === 'connected'
  // If the active call is NOT for this conversation, we might want to disable the buttons or show a different state.
  // But for now, if there is ANY active call, we can just disable the buttons to prevent multiple calls.
  const busy = call.phase !== 'idle'
  const canPressCall = canCall && hubReady && !busy

  const callDisabledTitle = busy 
    ? t('chat.calls.busy') 
    : !canCall
    ? t('chat.calls.directOnly')
    : !hubReady
      ? t('chat.calls.hubNotConnected')
      : undefined

  return (
    <div className="flex items-center gap-1">
      <button
        type="button"
        title={callDisabledTitle}
        disabled={!canPressCall}
        onClick={() => {
          if (convId && remoteUserId) call.startVoice(convId, remoteUserId)
        }}
        className={`flex h-10 w-10 items-center justify-center rounded-full transition-colors ${
          canPressCall
            ? 'text-blue-500 hover:bg-slate-100 hover:text-blue-600'
            : 'text-slate-300'
        }`}
        aria-label={t('chat.calls.voice')}
      >
        <Phone className="h-5 w-5" />
      </button>

      <button
        type="button"
        title={callDisabledTitle}
        disabled={!canPressCall}
        onClick={() => {
          if (convId && remoteUserId) call.startVideo(convId, remoteUserId)
        }}
        className={`flex h-10 w-10 items-center justify-center rounded-full transition-colors ${
          canPressCall
            ? 'text-blue-500 hover:bg-slate-100 hover:text-blue-600'
            : 'text-slate-300'
        }`}
        aria-label={t('chat.calls.video')}
      >
        <Video className="h-6 w-6" />
      </button>
    </div>
  )
}
