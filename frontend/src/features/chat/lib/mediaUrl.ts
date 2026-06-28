const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5034'

/**
 * Turn stored chat file URLs into absolute URLs for `<audio>` / `<img>` / links.
 * Rewrites any absolute URL whose path is under `/uploads/` to use `API_URL`, so
 * wrong ports in DB (e.g. localhost:5000 vs 5034) still load from the running API.
 */
export function resolveChatAssetUrl(url: string): string {
  const u = url?.trim() ?? ''
  if (!u) return ''

  if (u.startsWith('http://') || u.startsWith('https://')) {
    try {
      const parsed = new URL(u)
      const loopback = parsed.hostname === 'localhost' || parsed.hostname === '127.0.0.1'
      // Fix wrong port in DB (e.g. Chat:BaseUrl was localhost:5000 while API runs on 5034).
      // Do not rewrite real CDN / MinIO hosts.
      if (loopback && parsed.pathname.includes('/uploads/')) {
        return `${API_URL}${parsed.pathname}${parsed.search}`
      }
    } catch {
      /* ignore malformed */
    }
    return u
  }

  if (u.startsWith('/')) return `${API_URL}${u}`
  return `${API_URL}/${u}`
}
