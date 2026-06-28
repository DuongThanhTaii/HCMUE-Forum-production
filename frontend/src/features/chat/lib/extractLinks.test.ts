import { describe, expect, it } from 'vitest'
import { extractLinks } from './extractLinks'

describe('extractLinks', () => {
  it('extracts http and www URLs with dedupe', () => {
    const urls = extractLinks('Xem https://example.com/a và www.uni.edu.vn/path nhé.')
    expect(urls).toHaveLength(2)
    expect(urls).toContain('https://example.com/a')
    expect(urls).toContain('https://www.uni.edu.vn/path')
  })

  it('trims trailing punctuation for Vietnamese sentences', () => {
    const urls = extractLinks('Link: https://docs.microsoft.com/ef/core.')
    expect(urls).toEqual(['https://docs.microsoft.com/ef/core'])
  })
})
