/**
 * Dock visibility persisted per browser profile.
 * Cross-tab sync is not implemented (spec §4.2); consider BroadcastChannel later.
 */

const KEY_VISIBILITY = 'chat:dock:visibility'
const KEY_OPEN_THREADS = 'chat:dock:openThreads'

export type DockVisibility = 'hidden' | 'visible'

export function getDockVisibility(): DockVisibility {
  if (typeof localStorage === 'undefined') return 'visible'
  const v = localStorage.getItem(KEY_VISIBILITY)
  return v === 'hidden' ? 'hidden' : 'visible'
}

export function setDockVisibility(v: DockVisibility): void {
  if (typeof localStorage === 'undefined') return
  localStorage.setItem(KEY_VISIBILITY, v)
}

export function getDockOpenThreads(): string[] {
  if (typeof localStorage === 'undefined') return []
  try {
    const raw = localStorage.getItem(KEY_OPEN_THREADS)
    if (!raw) return []
    const parsed = JSON.parse(raw) as unknown
    return Array.isArray(parsed) ? parsed.map(String) : []
  } catch {
    return []
  }
}

export function setDockOpenThreads(ids: string[]): void {
  if (typeof localStorage === 'undefined') return
  localStorage.setItem(KEY_OPEN_THREADS, JSON.stringify(ids))
}
