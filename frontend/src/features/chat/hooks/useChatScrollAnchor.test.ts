import { describe, expect, it } from 'vitest'
import { isAtBottom } from '../lib/scrollConstants'

describe('isAtBottom', () => {
  it('returns true when scroll is at the bottom', () => {
    expect(isAtBottom(950, 1000, 50, 48)).toBe(true)
  })

  it('returns false when user scrolled up', () => {
    expect(isAtBottom(400, 1000, 50, 48)).toBe(false)
  })

  it('returns true within threshold', () => {
    expect(isAtBottom(902, 1000, 50, 48)).toBe(true)
  })
})
