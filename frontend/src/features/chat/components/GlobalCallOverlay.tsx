import { useMemo } from 'react'
import { useCallContext } from '../context/CallContext'
import { ChatCallOverlay } from './ChatCallOverlay'
import { chatApi } from '../api/chat.api'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { primaryConversationTitle } from '../lib/conversationLabels'

export function GlobalCallOverlay() {
  const call = useCallContext()
  const { activeConversationId } = call
  const currentUserId = useAppSelector((s) => s.auth.user?.id ?? null)

  const { data: convos } = chatApi.useGetConversationsQuery(undefined, {
    skip: !currentUserId,
  })

  const conversation = useMemo(() => {
    return convos?.find((c) => c.id === activeConversationId) || null
  }, [convos, activeConversationId])

  if (!call || call.phase === 'idle') return null

  const remoteLabel =
    conversation && currentUserId
      ? primaryConversationTitle(conversation, currentUserId)
      : 'User'

  return (
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
  )
}
