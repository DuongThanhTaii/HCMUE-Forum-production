import {
  useCallback,
  useLayoutEffect,
  useRef,
  useState,
  type UIEventHandler,
} from 'react'
import {
  BOTTOM_THRESHOLD_PX,
  isAtBottom as computeAtBottom,
  LOAD_MORE_TOP_PX,
} from '../lib/scrollConstants'
import { scrollElementWithinContainer } from '../lib/scrollWithinContainer'

export type MessageListChangeMode = 'initial' | 'append' | 'prepend' | 'replace'

export type UseChatScrollAnchorOptions = {
  onNearTop?: () => void
  hasMoreOlder?: boolean
  isLoadingOlder?: boolean
}

export function useChatScrollAnchor(
  messageCount: number,
  conversationKey: string | null,
  options: UseChatScrollAnchorOptions = {},
) {
  const { onNearTop, hasMoreOlder = false, isLoadingOlder = false } = options

  const containerRef = useRef<HTMLDivElement | null>(null)
  const [atBottom, setAtBottom] = useState(true)
  const [pendingNewCount, setPendingNewCount] = useState(0)

  const prevCountRef = useRef(0)
  const prevKeyRef = useRef<string | null>(null)
  const pendingPrependRef = useRef(false)
  const loadOlderCooldownRef = useRef(false)

  const prefersReducedMotion = useCallback(() => {
    if (typeof window === 'undefined') return false
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches
  }, [])

  const scrollToBottom = useCallback((smooth = false) => {
    const el = containerRef.current
    if (!el) return
    const behavior = smooth && !prefersReducedMotion() ? 'smooth' : 'auto'
    el.scrollTo({ top: el.scrollHeight, behavior })
    setPendingNewCount(0)
    setAtBottom(true)
  }, [prefersReducedMotion])

  const handleScroll: UIEventHandler<HTMLDivElement> = useCallback(
    (event) => {
      const el = event.currentTarget
      const bottom = computeAtBottom(el.scrollTop, el.scrollHeight, el.clientHeight)
      setAtBottom(bottom)
      if (bottom) {
        setPendingNewCount(0)
      }

      if (
        el.scrollTop <= LOAD_MORE_TOP_PX &&
        hasMoreOlder &&
        !isLoadingOlder &&
        !loadOlderCooldownRef.current &&
        onNearTop
      ) {
        loadOlderCooldownRef.current = true
        onNearTop()
        window.setTimeout(() => {
          loadOlderCooldownRef.current = false
        }, 300)
      }
    },
    [hasMoreOlder, isLoadingOlder, onNearTop],
  )

  const onUserSentMessage = useCallback(() => {
    scrollToBottom(true)
  }, [scrollToBottom])

  const markPrependPending = useCallback(() => {
    pendingPrependRef.current = true
  }, [])

  useLayoutEffect(() => {
    if (conversationKey !== prevKeyRef.current) {
      prevKeyRef.current = conversationKey
      prevCountRef.current = 0
      pendingPrependRef.current = false
      setPendingNewCount(0)
    }
  }, [conversationKey])

  useLayoutEffect(() => {
    if (!conversationKey) return
    requestAnimationFrame(() => {
      scrollToBottom(false)
      requestAnimationFrame(() => scrollToBottom(false))
    })
  }, [conversationKey, scrollToBottom])

  useLayoutEffect(() => {
    const el = containerRef.current
    if (!el || messageCount === 0) {
      prevCountRef.current = messageCount
      return
    }

    const prev = prevCountRef.current
    const delta = messageCount - prev

    if (prev === 0) {
      scrollToBottom(false)
      requestAnimationFrame(() => {
        scrollToBottom(false)
        requestAnimationFrame(() => scrollToBottom(false))
      })
    } else if (delta > 0 && pendingPrependRef.current) {
      const newHeight = el.scrollHeight
      const prevHeight = el.dataset.prevScrollHeight
        ? Number(el.dataset.prevScrollHeight)
        : newHeight
      const prevTop = el.dataset.prevScrollTop ? Number(el.dataset.prevScrollTop) : el.scrollTop
      el.scrollTop = prevTop + (newHeight - prevHeight)
      pendingPrependRef.current = false
    } else if (delta > 0) {
      const bottom = computeAtBottom(el.scrollTop, el.scrollHeight, el.clientHeight)
      if (bottom) {
        scrollToBottom(true)
      } else {
        setPendingNewCount((c) => c + delta)
        setAtBottom(false)
      }
    } else if (delta < 0) {
      scrollToBottom(false)
    }

    prevCountRef.current = messageCount
    delete el.dataset.prevScrollHeight
    delete el.dataset.prevScrollTop
  }, [messageCount, scrollToBottom])

  const captureScrollBeforePrepend = useCallback(() => {
    const el = containerRef.current
    if (!el) return
    el.dataset.prevScrollHeight = String(el.scrollHeight)
    el.dataset.prevScrollTop = String(el.scrollTop)
    pendingPrependRef.current = true
  }, [])

  const scrollToMessageId = useCallback(
    (messageId: string) => {
      const el = containerRef.current
      if (!el) return false
      const escaped =
        typeof CSS !== 'undefined' && 'escape' in CSS
          ? CSS.escape(messageId)
          : messageId.replace(/"/g, '\\"')
      const target = el.querySelector(`[data-message-id="${escaped}"]`)
      if (!(target instanceof HTMLElement)) return false
      scrollElementWithinContainer(el, target, 'center')
      const bottom = computeAtBottom(el.scrollTop, el.scrollHeight, el.clientHeight)
      setAtBottom(bottom)
      if (bottom) {
        setPendingNewCount(0)
      }
      return true
    },
    [prefersReducedMotion],
  )

  return {
    containerRef,
    atBottom,
    pendingNewCount,
    scrollToBottom,
    scrollToMessageId,
    handleScroll,
    onUserSentMessage,
    markPrependPending,
    captureScrollBeforePrepend,
    bottomThresholdPx: BOTTOM_THRESHOLD_PX,
  }
}
