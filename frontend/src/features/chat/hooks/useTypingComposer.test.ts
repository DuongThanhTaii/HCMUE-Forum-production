import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useTypingComposer } from './useTypingComposer'
import { TYPING_DEBOUNCE_MS, TYPING_IDLE_STOP_MS } from '../constants/typingThrottle'

describe('useTypingComposer', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('emits typing true after debounce then false after idle', async () => {
    const sendTyping = vi.fn()
    const { result } = renderHook(() =>
      useTypingComposer({
        enabled: true,
        conversationId: 'c1',
        sendTyping,
      })
    )

    act(() => {
      result.current.onComposerChange('hi')
    })
    expect(sendTyping).not.toHaveBeenCalled()

    await act(async () => {
      vi.advanceTimersByTime(TYPING_DEBOUNCE_MS)
    })
    expect(sendTyping).toHaveBeenCalledWith('c1', true)

    await act(async () => {
      vi.advanceTimersByTime(TYPING_IDLE_STOP_MS)
    })
    expect(sendTyping).toHaveBeenCalledWith('c1', false)
  })

  it('flushStop sends false when burst was active', async () => {
    const sendTyping = vi.fn()
    const { result } = renderHook(() =>
      useTypingComposer({
        enabled: true,
        conversationId: 'c1',
        sendTyping,
      })
    )

    act(() => {
      result.current.onComposerChange('x')
    })
    await act(async () => {
      vi.advanceTimersByTime(TYPING_DEBOUNCE_MS)
    })

    act(() => {
      result.current.flushStop()
    })
    expect(sendTyping).toHaveBeenCalledWith('c1', false)
  })
})
