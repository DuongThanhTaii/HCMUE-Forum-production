import type { ReactNode } from 'react'

interface LogsFilterBarProps {
  children: ReactNode
  /** Full grid/layout classes; when omitted, defaults to 1 col / lg:4 cols. */
  className?: string
}

export function LogsFilterBar({ children, className }: LogsFilterBarProps) {
  return (
    <section
      className={
        className ??
        'grid grid-cols-1 gap-3 rounded-xl border border-slate-200 bg-white p-4 lg:grid-cols-4'
      }
    >
      {children}
    </section>
  )
}
