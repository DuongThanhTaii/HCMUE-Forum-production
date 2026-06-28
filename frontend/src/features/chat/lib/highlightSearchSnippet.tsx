import type { ReactNode } from 'react'

export function highlightSearchSnippet(snippet: string, query: string): ReactNode {
  const trimmed = query.trim()
  if (!trimmed) {
    return snippet
  }

  const lowerSnippet = snippet.toLowerCase()
  const lowerQuery = trimmed.toLowerCase()
  const idx = lowerSnippet.indexOf(lowerQuery)
  if (idx < 0) {
    return snippet
  }

  const before = snippet.slice(0, idx)
  const match = snippet.slice(idx, idx + trimmed.length)
  const after = snippet.slice(idx + trimmed.length)

  return (
    <>
      {before}
      <mark className="rounded bg-amber-100 px-0.5 text-slate-900">{match}</mark>
      {after}
    </>
  )
}
