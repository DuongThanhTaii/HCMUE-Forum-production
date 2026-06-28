import { useState, useMemo } from 'react'
import { ChevronDown, ChevronRight, ShieldAlert } from 'lucide-react'
import type { PermissionDto, RoleDto } from '../../types/admin.types'

type RolePermissionGridProps = {
  permissions: PermissionDto[]
  selectedRole: RoleDto | null
  noRoleText: string
  isPermissionAssigned: (roleId: string, permissionId: string) => boolean
  onTogglePermission: (permissionId: string) => void | Promise<void>
  isBusy: boolean
}

export function RolePermissionGrid({
  permissions,
  selectedRole,
  noRoleText,
  isPermissionAssigned,
  onTogglePermission,
  isBusy,
}: RolePermissionGridProps) {
  const [expandedModules, setExpandedModules] = useState<Record<string, boolean>>({})

  const toggleModule = (module: string) => {
    setExpandedModules((prev) => ({
      ...prev,
      [module]: !prev[module],
    }))
  }

  // Group permissions by module
  const groupedPermissions = useMemo(() => {
    const groups: Record<string, PermissionDto[]> = {}
    for (const p of permissions) {
      const mod = p.module || p.code.split('.')[0] || 'Other'
      if (!groups[mod]) groups[mod] = []
      groups[mod].push(p)
    }
    return groups
  }, [permissions])

  if (!selectedRole) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-4 text-sm text-slate-600">
        {noRoleText}
      </div>
    )
  }

  const isSystemRole = selectedRole.isSystemRole

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="mb-6 flex items-start justify-between">
        <div>
          <h2 className="text-xl font-bold text-slate-900">{selectedRole.name}</h2>
          <p className="mt-1 text-sm text-slate-500">{selectedRole.description || 'Không có mô tả'}</p>
        </div>
        {isSystemRole && (
          <span className="inline-flex items-center gap-1 rounded-full bg-amber-100 px-2.5 py-1 text-xs font-medium text-amber-800">
            <ShieldAlert className="h-3.5 w-3.5" />
            System Role
          </span>
        )}
      </div>

      <div className="space-y-4">
        {Object.entries(groupedPermissions).map(([moduleName, perms]) => {
          const isExpanded = expandedModules[moduleName] !== false
          const assignedCount = perms.filter((p) => isPermissionAssigned(selectedRole.id, p.id)).length
          const totalCount = perms.length

          return (
            <div key={moduleName} className="overflow-hidden rounded-lg border border-slate-200">
              <button
                type="button"
                onClick={() => toggleModule(moduleName)}
                className="flex w-full items-center justify-between bg-slate-50 px-4 py-3 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-inset"
              >
                <div className="flex items-center gap-2">
                  {isExpanded ? (
                    <ChevronDown className="h-5 w-5 text-slate-400" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-slate-400" />
                  )}
                  <span className="font-semibold text-slate-700 capitalize">{moduleName}</span>
                </div>
                <div className="flex min-w-0 flex-1 items-center justify-end gap-3 sm:max-w-md">
                  <div className="h-2 min-w-[7rem] flex-1 overflow-hidden rounded-full bg-slate-200 sm:min-w-[10rem]">
                    <div
                      className="h-full rounded-full bg-gradient-to-r from-emerald-500 to-emerald-400 shadow-[inset_0_1px_0_rgba(255,255,255,0.35)] transition-all duration-300 ease-out"
                      style={{ width: `${totalCount ? (assignedCount / totalCount) * 100 : 0}%` }}
                      role="progressbar"
                      aria-valuenow={assignedCount}
                      aria-valuemin={0}
                      aria-valuemax={totalCount}
                      aria-label={`${moduleName}: ${assignedCount} trên ${totalCount}`}
                    />
                  </div>
                  <span className="shrink-0 tabular-nums text-sm font-semibold text-emerald-700">
                    {assignedCount}/{totalCount}
                    <span className="ml-1.5 font-normal text-emerald-600/90">
                      ({totalCount ? Math.round((assignedCount / totalCount) * 100) : 0}%)
                    </span>
                  </span>
                </div>
              </button>

              {isExpanded && (
                <div className="grid grid-cols-1 gap-px bg-slate-200 sm:grid-cols-2 lg:grid-cols-3">
                  {perms.map((permission) => {
                    const checked = isPermissionAssigned(selectedRole.id, permission.id)
                    return (
                      <label
                        key={permission.id}
                        className={`flex cursor-pointer items-start gap-3 bg-white p-4 transition-colors hover:bg-slate-50 ${isSystemRole ? 'opacity-70 cursor-not-allowed' : ''}`}
                        title={permission.description}
                      >
                        <div className="flex h-5 items-center">
                          <input
                            type="checkbox"
                            checked={checked}
                            disabled={isBusy || isSystemRole}
                            onChange={() => {
                              void onTogglePermission(permission.id)
                            }}
                            aria-label={permission.name}
                            className="h-4 w-4 rounded border-slate-300 text-primary-600 focus:ring-primary-600 disabled:opacity-50"
                          />
                        </div>
                        <div className="flex flex-col">
                          <span className="text-sm font-medium text-slate-900">{permission.name}</span>
                          <span className="mt-1 font-mono text-[11px] text-slate-500 tracking-tight">
                            {permission.code}
                          </span>
                        </div>
                      </label>
                    )
                  })}
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
