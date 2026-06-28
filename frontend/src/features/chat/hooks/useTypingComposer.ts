import { useCallback, useRef } from 'react'
import { TYPING_DEBOUNCE_MS, TYPING_IDLE_STOP_MS } from '../constants/typingThrottle'

type Params = {
  enabled: boolean
  conversationId: string | null
  sendTyping: (conversationId: string, isTyping: boolean) => void | Promise<void>
}

export function useTypingComposer({ enabled, conversationId, sendTyping }: Params) {
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null)
  const idleTimer = useRef<ReturnType<typeof setTimeout> | null>(null)
  const typingTrueSent = useRef(false)

  const clearTimers = useCallback(() => {
    if (debounceTimer.current) clearTimeout(debounceTimer.current)
    if (idleTimer.current) clearTimeout(idleTimer.current)
    debounceTimer.current = null
    idleTimer.current = null
  }, [])

  const flushStop = useCallback(() => {
    clearTimers()
    if (enabled && conversationId && typingTrueSent.current) {
      typingTrueSent.current = false
      void sendTyping(conversationId, false)
    }
  }, [clearTimers, conversationId, enabled, sendTyping])

  const onComposerChange = useCallback(
    (text: string) => {
      if (!enabled || !conversationId) return
      clearTimers()

      if (!text.trim()) {
        flushStop()
        return
      }

      debounceTimer.current = setTimeout(() => {
        if (!typingTrueSent.current) {
          typingTrueSent.current = true
          void sendTyping(conversationId, true)
        }
        debounceTimer.current = null
      }, TYPING_DEBOUNCE_MS)

      idleTimer.current = setTimeout(() => {
        typingTrueSent.current = false
        void sendTyping(conversationId, false)
        idleTimer.current = null
      }, TYPING_IDLE_STOP_MS)
    },
    [clearTimers, conversationId, enabled, flushStop, sendTyping]
  )

  return { onComposerChange, flushStop }
}
