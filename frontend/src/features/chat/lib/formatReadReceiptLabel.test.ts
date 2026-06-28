import { describe, expect, it } from 'vitest'
import { formatReadReceiptLabel, readReceiptCountExcludingSelf } from './formatReadReceiptLabel'

describe('formatReadReceiptLabel', () => {
  it('returns null when only self read', () => {
    expect(
      formatReadReceiptLabel(
        [{ userId: 'me', readAt: '2026-01-01' }],
        'me',
        'peer',
      ),
    ).toBeNull()
  })

  it('returns seen when peer read', () => {
    expect(
      formatReadReceiptLabel(
        [
          { userId: 'me', readAt: '2026-01-01' },
          { userId: 'peer', readAt: '2026-01-02' },
        ],
        'me',
        'peer',
      ),
    ).toBe('seen')
  })

  it('counts others excluding self', () => {
    expect(
      readReceiptCountExcludingSelf(
        [{ userId: 'a', readAt: '' }, { userId: 'me', readAt: '' }],
        'me',
      ),
    ).toBe(1)
  })
})
