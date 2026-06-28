import { useTranslation } from 'react-i18next'
import { Phone, Video } from 'lucide-react'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import { useChatContext } from '../context/ChatContext'
import { useWebRtcCall } from '../hooks/useWebRtcCall'
import { primaryConversationTitle } from '../lib/conversationLabels'
import type { ChatThreadRef, ConversationDto } from '../types/chat.types'
import { ChatCallOverlay } from './ChatCallOverlay'

type Props = {
  threadRef: ChatThreadRef
  conversation: ConversationDto | null
}

/**
 * 1:1 WebRTC (voice / video / screen) with SignalR relay.
 * Rules-of-Hooks: all hooks run unconditionally; convId defaults to '' when not a conversation.
 */
export function ChatCallBar({ threadRef, conversation }: Props) {
  const { t } = useTranslation()
  const { hubStatus } = useChatContext()
  const currentUserId = useAppSelector(selectUserId)

  const isConversation = threadRef.kind === 'conversation'
  const convId = isConversation ? threadRef.conversationId : ''
  const remoteUserId =
    conversation?.directPeerUserId ??
    (conversation?.participantIds.find((id) => id !== currentUserId) ?? null)

  const isPair = (conversation?.participantIds.length ?? 0) === 2
  const canCall = isConversation && Boolean(conversation && isPair && remoteUserId)

  // Hook always called — never after a conditional return
  const call = useWebRtcCall({
    conversationId: convId,
    remoteUserId: remoteUserId ?? null,
    canCall,
  })

  // Channel threads: render nothing (overlay still mounts but stays idle/null)
  if (!isConversation) return null

  const remoteLabel =
    conversation && currentUserId
      ? primaryConversationTitle(conversation, currentUserId)
      : t('chat.title')

  const hubReady = hubStatus === 'connected'
  const busy = call.phase !== 'idle'
  const canPressCall = canCall && hubReady

  const callDisabledTitle = !canCall
    ? t('chat.calls.directOnly')
    : !hubReady
      ? t('chat.calls.hubNotConnected')
      : undefined

  return (
    <>
      <div className="mb-2 flex flex-wrap gap-2">
        <button
          type="button"
          disabled={!canPressCall || busy}
          onClick={() => void call.startVoice()}
          className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-xs text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          title={callDisabledTitle}
        >
          <Phone className="h-3.5 w-3.5" aria-hidden />
          {t('chat.calls.voice')}
        </button>
        <button
          type="button"
          disabled={!canPressCall || busy}
          onClick={() => void call.startVideo()}
          className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-xs text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          title={callDisabledTitle}
        >
          <Video className="h-3.5 w-3.5" aria-hidden />
          {t('chat.calls.video')}
        </button>
      </div>

      <ChatCallOverlay
        phase={call.phase}
        callMode={call.callMode}
        remoteLabel={remoteLabel}
        localStream={call.localStream}
        remoteStream={call.remoteStream}
        error={call.error}
        muted={call.muted}
        onAccept={() => void call.acceptIncoming()}
        onReject={() => void call.rejectIncoming()}
        onEnd={() => void call.endCall()}
        onToggleMute={() => call.toggleMute()}
        onShareScreen={() => void call.replaceWithScreen()}
        onStopScreenShare={() => void call.stopScreenShare()}
        onDismissError={() => call.dismissError()}
        incomingFromName={call.incoming?.fromUserName ?? null}
      />
    </>
  )
}
