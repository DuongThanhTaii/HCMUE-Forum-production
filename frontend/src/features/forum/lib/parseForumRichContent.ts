/**
 * Forum post/comment content may end with a block:
 *   Attachments:
 *   - https://...
 * Image URLs (incl. Cloudinary /image/upload/...) are surfaced for inline rendering.
 */

export type ParsedForumContent = {
  body: string
  imageUrls: string[]
  fileUrls: string[]
}

const IMAGE_EXT_RE = /\.(png|jpe?g|gif|webp|bmp|svg|avif)(\?.*)?$/i
const URL_RE = /https?:\/\/[^\s<>"')\]]+/gi

function toValidHttpUrl(input: string): string | null {
  const cleaned = input.trim().replace(/[),.;]+$/g, '')
  if (!cleaned) return null
  try {
    const u = new URL(cleaned)
    if (u.protocol !== 'http:' && u.protocol !== 'https:') return null
    return u.toString()
  } catch {
    return null
  }
}

export function isLikelyImageUrl(url: string): boolean {
  const lower = url.toLowerCase()
  return IMAGE_EXT_RE.test(lower) || lower.includes('/image/upload/')
}

export function parseForumRichContent(raw: string): ParsedForumContent {
  const normalized = (raw ?? '').replace(/\r\n/g, '\n').trim()
  if (!normalized) {
    return { body: '', imageUrls: [], fileUrls: [] }
  }

  const markerIndex = normalized.toLowerCase().indexOf('attachments:')
  let body = markerIndex >= 0 ? normalized.slice(0, markerIndex).trim() : normalized
  const attachmentSection = markerIndex >= 0 ? normalized.slice(markerIndex + 'attachments:'.length) : ''

  const urls = new Set<string>()
  for (const match of attachmentSection.match(URL_RE) ?? []) {
    const parsed = toValidHttpUrl(match)
    if (parsed) urls.add(parsed)
  }

  for (const match of body.match(URL_RE) ?? []) {
    const parsed = toValidHttpUrl(match)
    if (parsed && isLikelyImageUrl(parsed)) urls.add(parsed)
  }

  const imageUrls: string[] = []
  const fileUrls: string[] = []
  for (const url of urls) {
    if (isLikelyImageUrl(url)) imageUrls.push(url)
    else fileUrls.push(url)
  }

  return { body, imageUrls, fileUrls }
}
