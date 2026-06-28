export type CareerStructuredDescription = {
  overview: string[]
  profile: string[]
  benefits: string[]
  additional: string[]
  images: string[]
}

const MARKERS = {
  overview: ['## Job Overview', '## Mô tả công việc'],
  profile: ['## Candidate Profile', '## Hồ sơ của bạn'],
  benefits: ['## Benefits', '## Quyền lợi'],
  additional: ['## Contact & Additional Info', '## Liên hệ & thông tin bổ sung'],
  images: ['## Visual Assets', '## Hình ảnh'],
}

const IMAGE_URL_RE = /^https?:\/\/\S+$/i

function toLines(value: string): string[] {
  return value
    .split('\n')
    .map((line) => line.trim())
    .filter(Boolean)
    .map((line) => line.replace(/^[-*]\s*/, '').trim())
    .filter(Boolean)
}

function findMarker(source: string, candidates: string[]) {
  const indexes = candidates
    .map((marker) => ({ marker, idx: source.indexOf(marker) }))
    .filter((item) => item.idx >= 0)
    .sort((a, b) => a.idx - b.idx)
  return indexes[0] ?? null
}

function extractBetween(source: string, startCandidates: string[], endCandidates: string[][]): string {
  const start = findMarker(source, startCandidates)
  if (!start) return ''

  const from = start.idx + start.marker.length
  let to = source.length
  for (const group of endCandidates) {
    const next = findMarker(source.slice(from), group)
    if (next) {
      to = Math.min(to, from + next.idx)
    }
  }
  return source.slice(from, to).trim()
}

export function parseCareerDescription(raw: string | null | undefined): CareerStructuredDescription {
  const text = (raw ?? '').trim()
  if (!text) {
    return { overview: [], profile: [], benefits: [], additional: [], images: [] }
  }

  const hasStructured = Object.values(MARKERS).some((group) => group.some((marker) => text.includes(marker)))
  if (!hasStructured) {
    return {
      overview: toLines(text),
      profile: [],
      benefits: [],
      additional: [],
      images: [],
    }
  }

  const overview = toLines(
    extractBetween(text, MARKERS.overview, [MARKERS.profile, MARKERS.benefits, MARKERS.additional, MARKERS.images]),
  )
  const profile = toLines(
    extractBetween(text, MARKERS.profile, [MARKERS.benefits, MARKERS.additional, MARKERS.images]),
  )
  const benefits = toLines(
    extractBetween(text, MARKERS.benefits, [MARKERS.additional, MARKERS.images]),
  )
  const additional = toLines(extractBetween(text, MARKERS.additional, [MARKERS.images]))
  const imageLines = toLines(extractBetween(text, MARKERS.images, []))
  const images = imageLines.filter((line) => IMAGE_URL_RE.test(line))

  return { overview, profile, benefits, additional, images }
}

export function composeCareerDescription(input: {
  overview: string
  profile: string
  benefits: string
  additional: string
  imageUrls: string[]
}) {
  const normalize = (value: string) => toLines(value).join('\n')
  const imageLines = input.imageUrls
    .map((url) => url.trim())
    .filter((url) => IMAGE_URL_RE.test(url))
    .map((url) => `- ${url}`)
    .join('\n')

  return [
    '## Job Overview',
    normalize(input.overview),
    '',
    '## Candidate Profile',
    normalize(input.profile),
    '',
    '## Benefits',
    normalize(input.benefits),
    '',
    '## Contact & Additional Info',
    normalize(input.additional),
    '',
    '## Visual Assets',
    imageLines || '-',
  ].join('\n')
}
