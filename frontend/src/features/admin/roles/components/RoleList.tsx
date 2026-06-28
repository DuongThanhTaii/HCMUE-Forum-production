import type { RoleDto } from '../../types/admin.types'

type RoleListProps = {
  roles: RoleDto[]
  selectedRoleId: string | null
  onSelectRole: (roleId: string) => void
  /** Tổng số quyền trong hệ thống — để hiển thị tỷ lệ gán cho từng vai trò */
  totalPermissionsInSystem: number
}

export function RoleList({
  roles,
  selectedRoleId,
  onSelectRole,
  totalPermissionsInSystem,
}: RoleListProps) {
  return (
    <div className="space-y-2">
      {roles.map((role) => {
        const isSelected = role.id === selectedRoleId
        const assigned = role.permissionCount ?? 0
        const total = totalPermissionsInSystem
        const pct = total > 0 ? Math.min(100, Math.round((assigned / total) * 100)) : 0
        return (
          <button
            key={role.id}
            type="button"
            className={`w-full rounded-lg border px-3 py-2.5 text-left transition ${
              isSelected
                ? 'border-rose-300 bg-rose-50 text-rose-700 ring-1 ring-rose-200/80'
                : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
            }`}
            onClick={() => onSelectRole(role.id)}
          >
            <div className="flex items-start justify-between gap-2">
              <div className="min-w-0 flex-1">
                <p className="text-sm font-semibold">{role.name}</p>
                <p className="mt-1 line-clamp-2 text-xs text-slate-500">{role.description || '—'}</p>
              </div>
              <span className="shrink-0 tabular-nums text-[11px] font-semibold text-emerald-700">
                {assigned}/{total > 0 ? total : '—'}
              </span>
            </div>
            <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-slate-200">
              <div
                className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-teal-400 transition-[width] duration-300 ease-out"
                style={{ width: `${pct}%` }}
              />
            </div>
          </button>
        )
      })}
    </div>
  )
}
