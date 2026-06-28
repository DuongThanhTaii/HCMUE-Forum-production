import { useMemo, useState } from 'react'
import { Plus, X } from 'lucide-react'

export type AdminAssignListItem = {
  id: string
  label: string
  hint?: string
}

type AdminAssignListboxProps = {
  assignedIds: string[]
  items: AdminAssignListItem[]
  assignedTitle: string
  availableTitle: string
  emptyAssigned: string
  emptyAvailable: string
  searchPlaceholder?: string
  isBusy?: boolean
  onAssign: (id: string) => void | Promise<void>
  onRemove: (id: string) => void | Promise<void>
}

export function AdminAssignListbox({
  assignedIds,
  items,
  assignedTitle,
  availableTitle,
  emptyAssigned,
  emptyAvailable,
  searchPlaceholder,
  isBusy = false,
  onAssign,
  onRemove,
}: AdminAssignListboxProps) {
  const [query, setQuery] = useState('')
  const assignedSet = useMemo(() => new Set(assignedIds), [assignedIds])
  const normalizedQuery = query.trim().toLowerCase()

  const assigned = useMemo(
    () => items.filter((item) => assignedSet.has(item.id)),
    [items, assignedSet],
  )

  const available = useMemo(() => {
    return items.filter((item) => {
      if (assignedSet.has(item.id)) return false
      if (!normalizedQuery) return true
      return (
        item.label.toLowerCase().includes(normalizedQuery) ||
        item.hint?.toLowerCase().includes(normalizedQuery)
      )
    })
  }, [items, assignedSet, normalizedQuery])

  return (
    <div className="grid gap-3 sm:grid-cols-2">
      <div className="rounded-lg border border-slate-200 bg-slate-50/80">
        <div className="border-b border-slate-200 px-3 py-2">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">{assignedTitle}</p>
          <p className="mt-0.5 text-[11px] text-slate-500">{assigned.length}</p>
        </div>
        <ul className="max-h-52 space-y-1 overflow-y-auto p-2" aria-label={assignedTitle}>
          {assigned.map((item) => (
            <li key={item.id}>
              <div className="flex items-center justify-between gap-2 rounded-md border border-emerald-200 bg-white px-2.5 py-2">
                <div>
                  <p className="text-sm font-medium text-slate-900">{item.label}</p>
                  {item.hint ? <p className="text-[11px] text-slate-500">{item.hint}</p> : null}
                </div>
                <button
                  type="button"
                  disabled={isBusy}
                  onClick={() => void onRemove(item.id)}
                  className="cursor-pointer rounded p-1 text-slate-500 transition-colors hover:bg-rose-50 hover:text-rose-700 disabled:cursor-not-allowed disabled:opacity-50"
                  aria-label={`Gỡ ${item.label}`}
                >
                  <X className="h-4 w-4" aria-hidden />
                </button>
              </div>
            </li>
          ))}
          {!assigned.length ? (
            <li className="px-2 py-4 text-center text-xs text-slate-500">{emptyAssigned}</li>
          ) : null}
        </ul>
      </div>

      <div className="rounded-lg border border-slate-200 bg-white">
        <div className="border-b border-slate-200 px-3 py-2">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">{availableTitle}</p>
          {searchPlaceholder ? (
            <input
              type="search"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder={searchPlaceholder}
              className="mt-2 w-full rounded-md border border-slate-300 px-2 py-1.5 text-sm"
              disabled={isBusy}
            />
          ) : null}
        </div>
        <ul className="max-h-52 space-y-1 overflow-y-auto p-2" aria-label={availableTitle}>
          {available.map((item) => (
            <li key={item.id}>
              <button
                type="button"
                disabled={isBusy}
                onClick={() => void onAssign(item.id)}
                className="flex w-full cursor-pointer items-center justify-between gap-2 rounded-md border border-slate-200 px-2.5 py-2 text-left transition-colors hover:border-rose-200 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <div>
                  <p className="text-sm font-medium text-slate-900">{item.label}</p>
                  {item.hint ? <p className="text-[11px] text-slate-500">{item.hint}</p> : null}
                </div>
                <Plus className="h-4 w-4 shrink-0 text-rose-600" aria-hidden />
              </button>
            </li>
          ))}
          {!available.length ? (
            <li className="px-2 py-4 text-center text-xs text-slate-500">{emptyAvailable}</li>
          ) : null}
        </ul>
      </div>
    </div>
  )
}
