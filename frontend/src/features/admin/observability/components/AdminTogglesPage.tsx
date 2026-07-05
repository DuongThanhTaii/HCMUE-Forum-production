import { useMemo, useState } from 'react'
import { ChevronDown, ChevronRight, ShieldAlert } from 'lucide-react'
import { useAdminLogsPage } from '../hooks/useAdminLogsPage'
import { EndpointToggleRow } from './EndpointToggleRow'
import type { EndpointToggleDto } from '../../types/admin.types'

export function AdminTogglesPage() {
  const {
    t,
    toggles,
    isTogglesLoading,
    isTogglesError,
    isSetToggleLoading,
    submitToggle,
    maintenanceMode,
    isMaintenanceModeLoading,
    isSetMaintenanceModeLoading,
    submitMaintenanceMode,

    isFileUploadEnabled,
    isFileUploadSettingLoading,
    isSetSystemSettingLoading,
    submitFileUploadSetting,
  } = useAdminLogsPage()
  const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>({})
  const [maintenanceReason, setMaintenanceReason] = useState('')

  const regularToggles = useMemo(
    () => toggles.filter((item) => item.endpointKey !== 'System.Maintenance.Mode'),
    [toggles],
  )

  const groupedToggles = useMemo(() => {
    const groups: Record<string, EndpointToggleDto[]> = {}
    for (const t of regularToggles) {
      // Typically keys are "UniHub.Forum.GetPosts" -> prefix is "UniHub.Forum"
      const parts = t.endpointKey.split('.')
      let prefix = 'Other'
      if (parts.length >= 2) {
        prefix = parts.slice(0, 2).join('.')
      }
      if (!groups[prefix]) groups[prefix] = []
      groups[prefix].push(t)
    }
    return groups
  }, [regularToggles])

  const toggleGroup = (prefix: string) => {
    setExpandedGroups((prev) => ({
      ...prev,
      [prefix]: !prev[prefix],
    }))
  }

  if (isTogglesLoading) {
    return (
      <div className="flex h-40 items-center justify-center rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="h-6 w-6 animate-spin rounded-full border-b-2 border-primary-600"></div>
      </div>
    )
  }

  if (isTogglesError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-6 text-rose-700 shadow-sm">
        <div className="flex items-center gap-2">
          <ShieldAlert className="h-5 w-5" />
          <h3 className="font-semibold">{t('admin.togglesPage.messages.loadError')}</h3>
        </div>
      </div>
    )
  }

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <header className="mb-6 flex items-start justify-between border-b border-slate-100 pb-4">
        <div>
          <h1 className="text-xl font-bold text-slate-900">{t('admin.togglesPage.title')}</h1>
          <p className="mt-1 text-sm text-slate-500">{t('admin.togglesPage.subtitle')}</p>
          <p className="mt-1 text-xs text-slate-400">
            Danh sách hiển thị toàn bộ API controller routes; endpoint hệ thống (health/docs/control-plane) được bypass an toàn.
          </p>
        </div>
        <div className="rounded-lg bg-slate-50 px-3 py-2 text-center border border-slate-200">
          <p className="text-xs font-medium text-slate-500 uppercase tracking-wide">Total Enabled</p>
          <p className="text-xl font-bold text-slate-900">
            {regularToggles.filter((t) => t.isEnabled).length} <span className="text-sm font-normal text-slate-500">/ {regularToggles.length}</span>
          </p>
        </div>
      </header>

      <section className="mb-6 rounded-lg border border-slate-200 bg-slate-50 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-sm font-semibold text-slate-900">Maintenance Mode</h2>
            <p className="mt-1 text-xs text-slate-600">
              Bật để chuyển toàn bộ web sang chế độ bảo trì (API trả 503 cho hầu hết endpoint).
            </p>
            {maintenanceMode?.isEnabled && maintenanceMode.reason ? (
              <p className="mt-2 text-xs text-amber-700">Lý do: {maintenanceMode.reason}</p>
            ) : null}
          </div>
          <button
            type="button"
            role="switch"
            aria-checked={maintenanceMode?.isEnabled === true}
            disabled={isMaintenanceModeLoading || isSetMaintenanceModeLoading}
            onClick={() => {
              if (maintenanceMode?.isEnabled) {
                void submitMaintenanceMode(false, null)
                return
              }
              const reason = maintenanceReason.trim() || 'Scheduled maintenance'
              void submitMaintenanceMode(true, reason)
            }}
            className={`relative inline-flex h-6 w-11 flex-shrink-0 rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
              maintenanceMode?.isEnabled ? 'bg-emerald-600' : 'bg-slate-300'
            }`}
          >
            <span
              className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                maintenanceMode?.isEnabled ? 'translate-x-5' : 'translate-x-0'
              }`}
            />
          </button>
        </div>
        {!maintenanceMode?.isEnabled ? (
          <input
            value={maintenanceReason}
            onChange={(event) => setMaintenanceReason(event.target.value)}
            placeholder="Lý do maintenance (tuỳ chọn)"
            className="mt-3 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-xs text-slate-700 outline-none focus:border-primary"
          />
        ) : null}
      </section>

      <section className="mb-6 rounded-lg border border-slate-200 bg-slate-50 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-sm font-semibold text-slate-900">Cho phép Upload File (Tài liệu học tập)</h2>
            <p className="mt-1 text-xs text-slate-600">
              Bật để cho phép sinh viên upload trực tiếp file tài liệu học tập (lưu trữ trên server). Nếu tắt, sinh viên chỉ có thể gửi link Google Drive.
            </p>
          </div>
          <button
            type="button"
            role="switch"
            aria-checked={isFileUploadEnabled}
            disabled={isFileUploadSettingLoading || isSetSystemSettingLoading}
            onClick={() => {
              void submitFileUploadSetting(!isFileUploadEnabled)
            }}
            className={`relative inline-flex h-6 w-11 flex-shrink-0 rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${
              isFileUploadEnabled ? 'bg-emerald-600' : 'bg-slate-300'
            }`}
          >
            <span
              className={`pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                isFileUploadEnabled ? 'translate-x-5' : 'translate-x-0'
              }`}
            />
          </button>
        </div>
      </section>

      <section
        className={`space-y-4 transition-opacity ${
          maintenanceMode?.isEnabled ? 'pointer-events-none select-none opacity-60' : ''
        }`}
        aria-disabled={maintenanceMode?.isEnabled === true}
      >
        {maintenanceMode?.isEnabled ? (
          <div className="rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-xs font-medium text-amber-800">
            Maintenance đang bật: tạm khóa thao tác endpoint toggles bên dưới.
          </div>
        ) : null}
        {Object.entries(groupedToggles).map(([prefix, groupToggles]) => {
          const isExpanded = expandedGroups[prefix] !== false // Default open
          const enabledCount = groupToggles.filter((t) => t.isEnabled).length
          const totalCount = groupToggles.length
          const hasDisabled = enabledCount < totalCount

          return (
            <div key={prefix} className="overflow-hidden rounded-lg border border-slate-200">
              <button
                type="button"
                onClick={() => toggleGroup(prefix)}
                className="flex w-full items-center justify-between bg-slate-50 px-4 py-3 hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-inset"
              >
                <div className="flex items-center gap-2">
                  {isExpanded ? (
                    <ChevronDown className="h-5 w-5 text-slate-400" />
                  ) : (
                    <ChevronRight className="h-5 w-5 text-slate-400" />
                  )}
                  <span className="font-semibold text-slate-700">{prefix}</span>
                  {hasDisabled && (
                    <span className="ml-2 inline-flex items-center rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-800">
                      Alerts
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  <div className="h-1.5 w-24 overflow-hidden rounded-full bg-slate-200">
                    <div
                      className={`h-full transition-all ${hasDisabled ? 'bg-amber-500' : 'bg-primary-600'}`}
                      style={{ width: `${(enabledCount / totalCount) * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium text-slate-500">
                    {enabledCount}/{totalCount}
                  </span>
                </div>
              </button>

              {isExpanded && (
                <div className="divide-y divide-slate-100 bg-white">
                  {groupToggles.map((toggle) => (
                    <EndpointToggleRow
                      key={toggle.endpointKey}
                      toggle={toggle}
                      isSubmitting={isSetToggleLoading}
                      onSubmit={submitToggle}
                    />
                  ))}
                </div>
              )}
            </div>
          )
        })}

        {!regularToggles.length && (
          <div className="py-12 text-center text-sm text-slate-500">
            {t('admin.togglesPage.messages.empty')}
          </div>
        )}
      </section>
    </div>
  )
}
