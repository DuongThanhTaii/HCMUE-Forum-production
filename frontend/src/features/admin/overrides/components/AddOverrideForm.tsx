import { useMemo, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useGetCoursesQuery, useGetFacultiesQuery } from '@features/learning/api/learning.api'
import type { OverrideEffect, PermissionDto, PermissionOverrideDto } from '../../types/admin.types'

type AddOverrideFormProps = {
  permissions: PermissionDto[]
  overrides: PermissionOverrideDto[]
  isSubmitting: boolean
  onSubmit: (input: {
    permissionId: string
    scopeType: string
    scopeValue: string | null
    effect: OverrideEffect
    reason: string
    expiresAtUtc: string
  }) => Promise<void>
}

export function AddOverrideForm({ permissions, overrides, isSubmitting, onSubmit }: AddOverrideFormProps) {
  const { t } = useTranslation()
  const [permissionId, setPermissionId] = useState('')
  const [scopeType, setScopeType] = useState('Global')
  const [scopeValue, setScopeValue] = useState('')
  const [effect, setEffect] = useState<OverrideEffect>('Deny')
  const [reason, setReason] = useState('')
  const [expiresAtUtc, setExpiresAtUtc] = useState('')
  const [permissionQuery, setPermissionQuery] = useState('')
  const { data: faculties = [] } = useGetFacultiesQuery()
  const { data: coursesPaged } = useGetCoursesQuery({ page: 1, pageSize: 500 })
  const courses = coursesPaged?.items ?? []

  const normalizedScopeValue = scopeType === 'Global' ? null : scopeValue || null
  const normalizedQuery = permissionQuery.trim().toLowerCase()

  const existingAtScope = overrides.find(
    (item) =>
      !item.isRevoked &&
      item.permissionId === permissionId &&
      item.scopeType.toLowerCase() === scopeType.toLowerCase() &&
      (item.scopeValue ?? null) === normalizedScopeValue,
  )

  const availablePermissions = useMemo(() => {
    return permissions.filter((permission) => {
      const taken = overrides.some(
        (item) =>
          !item.isRevoked &&
          item.permissionId === permission.id &&
          item.scopeType.toLowerCase() === scopeType.toLowerCase() &&
          (item.scopeValue ?? null) === normalizedScopeValue,
      )
      if (taken) return false
      if (!normalizedQuery) return true
      return (
        permission.code.toLowerCase().includes(normalizedQuery) ||
        permission.name.toLowerCase().includes(normalizedQuery)
      )
    })
  }, [permissions, overrides, scopeType, normalizedScopeValue, normalizedQuery])

  const selectedPermission = permissions.find((p) => p.id === permissionId) ?? null

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    if (!permissionId) return
    await onSubmit({
      permissionId,
      scopeType,
      scopeValue: scopeType === 'Global' ? null : scopeValue,
      effect,
      reason,
      expiresAtUtc,
    })
    setPermissionId('')
    setReason('')
    setExpiresAtUtc('')
    setPermissionQuery('')
  }

  return (
    <form className="space-y-4 rounded-xl border border-slate-200 bg-white p-4" onSubmit={(event) => void submit(event)}>
      <h3 className="text-sm font-semibold text-slate-900">{t('admin.overridesPage.form.title')}</h3>

      <div className="grid gap-3 md:grid-cols-2">
        <label className="text-sm font-medium text-slate-700">
          {t('admin.overridesPage.form.scopeType')}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={scopeType}
            onChange={(event) => {
              setScopeType(event.target.value)
              setScopeValue('')
              setPermissionId('')
            }}
          >
            <option value="Global">{t('admin.overridesPage.form.scopeTypeOptions.global')}</option>
            <option value="Faculty">{t('admin.overridesPage.form.scopeTypeOptions.faculty')}</option>
            <option value="Course">{t('admin.overridesPage.form.scopeTypeOptions.course')}</option>
          </select>
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.overridesPage.form.scopeValue')}
          {scopeType === 'Global' ? (
            <input
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              value=""
              disabled
              placeholder={t('admin.overridesPage.form.scopeValuePlaceholderGlobal')}
            />
          ) : null}
          {scopeType === 'Faculty' ? (
            <select
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              value={scopeValue}
              onChange={(event) => {
                setScopeValue(event.target.value)
                setPermissionId('')
              }}
              required
            >
              <option value="">{t('admin.overridesPage.form.selectFaculty')}</option>
              {faculties.map((faculty) => (
                <option key={faculty.facultyId} value={faculty.facultyId}>
                  {faculty.name}
                </option>
              ))}
            </select>
          ) : null}
          {scopeType === 'Course' ? (
            <select
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              value={scopeValue}
              onChange={(event) => {
                setScopeValue(event.target.value)
                setPermissionId('')
              }}
              required
            >
              <option value="">{t('admin.overridesPage.form.selectCourse')}</option>
              {courses.map((course) => (
                <option key={course.courseId} value={course.courseId}>
                  {course.name}
                </option>
              ))}
            </select>
          ) : null}
        </label>
      </div>

      <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)]">
        <div className="rounded-lg border border-slate-200">
          <div className="border-b border-slate-200 px-3 py-2">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">
              {t('admin.overridesPage.form.pickPermission')}
            </p>
            <input
              type="search"
              value={permissionQuery}
              onChange={(e) => setPermissionQuery(e.target.value)}
              placeholder={t('admin.overridesPage.form.searchPermission')}
              className="mt-2 w-full rounded-md border border-slate-300 px-2 py-1.5 text-sm"
            />
          </div>
          <ul className="max-h-48 space-y-1 overflow-y-auto p-2" role="listbox" aria-label={t('admin.overridesPage.form.pickPermission')}>
            {availablePermissions.map((permission) => {
              const selected = permissionId === permission.id
              return (
                <li key={permission.id}>
                  <button
                    type="button"
                    onClick={() => setPermissionId(permission.id)}
                    className={`w-full cursor-pointer rounded-md border px-2.5 py-2 text-left transition-colors ${
                      selected
                        ? 'border-rose-300 bg-rose-50'
                        : 'border-slate-200 hover:border-rose-200 hover:bg-slate-50'
                    }`}
                    aria-selected={selected}
                  >
                    <p className="text-sm font-medium text-slate-900">{permission.name}</p>
                    <p className="font-mono text-[11px] text-slate-500">{permission.code}</p>
                  </button>
                </li>
              )
            })}
            {!availablePermissions.length ? (
              <li className="px-2 py-4 text-center text-xs text-slate-500">{t('admin.overridesPage.form.noPermissions')}</li>
            ) : null}
          </ul>
        </div>

        <div className="space-y-3 rounded-lg border border-slate-200 bg-slate-50/60 p-3">
          {!selectedPermission ? (
            <p className="text-sm text-slate-500">{t('admin.overridesPage.form.selectPermissionHint')}</p>
          ) : (
            <>
              <div>
                <p className="text-sm font-semibold text-slate-900">{selectedPermission.name}</p>
                <p className="font-mono text-xs text-slate-500">{selectedPermission.code}</p>
              </div>
              <p className="rounded-md border border-slate-200 bg-white px-2.5 py-2 text-xs text-slate-600">
                {existingAtScope
                  ? t('admin.overridesPage.form.effectStatusActive', { effect: existingAtScope.effect })
                  : t('admin.overridesPage.form.effectStatusFromRole')}
              </p>
              <label className="block text-sm font-medium text-slate-700">
                {t('admin.overridesPage.form.effect')}
                <select
                  className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm"
                  value={effect}
                  onChange={(event) => setEffect(event.target.value as OverrideEffect)}
                >
                  <option value="Allow">{t('admin.overridesPage.form.effectOptions.allow')}</option>
                  <option value="Deny">{t('admin.overridesPage.form.effectOptions.deny')}</option>
                </select>
              </label>
              <label className="block text-sm font-medium text-slate-700">
                {t('admin.overridesPage.form.reason')}
                <input
                  className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm"
                  value={reason}
                  onChange={(event) => setReason(event.target.value)}
                  placeholder={t('admin.overridesPage.form.reasonPlaceholder')}
                />
              </label>
              <label className="block text-sm font-medium text-slate-700">
                {t('admin.overridesPage.form.expiresAt')}
                <input
                  className="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm"
                  value={expiresAtUtc}
                  onChange={(event) => setExpiresAtUtc(event.target.value)}
                  placeholder={t('admin.overridesPage.form.expiresAtPlaceholder')}
                />
              </label>
            </>
          )}
        </div>
      </div>

      <button
        type="submit"
        className="cursor-pointer rounded-md bg-rose-600 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-rose-700 disabled:cursor-not-allowed disabled:opacity-60"
        disabled={isSubmitting || !permissionId}
      >
        {t('admin.overridesPage.form.submit')}
      </button>
    </form>
  )
}
