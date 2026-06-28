import { describe, expect, it } from 'vitest'
import { shouldBlockEnqueue } from './outboxPolicy'

describe('shouldBlockEnqueue', () => {
  it('returns false below cap', () => {
    expect(shouldBlockEnqueue(0, 50)).toBe(false)
    expect(shouldBlockEnqueue(49, 50)).toBe(false)
  })

  it('returns true at or above cap', () => {
    expect(shouldBlockEnqueue(50, 50)).toBe(true)
    expect(shouldBlockEnqueue(51, 50)).toBe(true)
  })
})
