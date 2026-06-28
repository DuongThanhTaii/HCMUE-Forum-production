const HTTP_URL_RE = /https?:\/\/[^\s<>"')\]]+/gi
const WWW_URL_RE = /\bwww\.[^\s<>"')\]]+/gi

function trimTrailingPunctuation(url: string): string {
  return url.replace(/[.,;:!?)\]}"']+$/u, '')
}

export function extractLinks(content: string): string[] {
  if (!content?.trim()) {
    return []
  }

  const seen = new Set<string>()
  const urls: string[] = []

  const add = (raw: string, prependHttps = false) => {
    const trimmed = trimTrailingPunctuation(raw.trim())
    if (!trimmed) return
    const url = prependHttps ? `https://${trimmed}` : trimmed
    const key = url.toLowerCase()
    if (seen.has(key)) return
    seen.add(key)
    urls.push(url)
  }

  for (const match of content.matchAll(HTTP_URL_RE)) {
    add(match[0])
  }
  for (const match of content.matchAll(WWW_URL_RE)) {
    add(match[0], true)
  }

  return urls
}

export function linkHost(url: string): string {
  try {
    return new URL(url).host
  } catch {
    return url
  }
}
