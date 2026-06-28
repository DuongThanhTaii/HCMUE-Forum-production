import { useMemo } from 'react'

function initialsFromName(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length >= 2) {
    return `${parts[0]![0] ?? ''}${parts[1]![0] ?? ''}`.toUpperCase()
  }
  if (parts.length === 1 && parts[0]!.length >= 2) {
    return parts[0]!.slice(0, 2).toUpperCase()
  }
  return (parts[0]?.[0] ?? '?').toUpperCase()
}

export function ChatPeerAvatar({
  name,
  className = '',
}: {
  name: string
  className?: string
}) {
  const initials = useMemo(() => initialsFromName(name || '?'), [name])

  return (
    <span
      className={`inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-violet-600 text-[13px] font-semibold text-white shadow-sm ${className ?? ''}`}
      aria-hidden
    >
      {initials}
    </span>
  )
}
