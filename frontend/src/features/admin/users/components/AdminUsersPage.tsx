import { Fragment } from 'react'
import { AdminAssignListbox } from '../../components/AdminAssignListbox'
import { useAdminUsersPage } from '../hooks/useAdminUsersPage'

export function AdminUsersPage() {
  const {
    t,
    users,
    roleOptions,
    statusOptions,
    roleListItems,
    searchValue,
    roleFilter,
    statusFilter,
    editingRolesUserId,
    roleActionError,
    isRoleMutating,
    isLoading,
    isError,
    setSearchValue,
    setRoleFilter,
    setStatusFilter,
    openRoleEditor,
    closeRoleEditor,
    assignRole,
    removeRole,
    getRoleLabels,
  } = useAdminUsersPage()

  if (isLoading) {
    return <div className="rounded-xl border border-slate-200 bg-white p-4">{t('common.loading')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">
        {t('admin.usersPage.messages.loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header className="rounded-xl border border-slate-200 bg-white p-4">
        <h1 className="text-lg font-semibold text-slate-900">{t('admin.usersPage.title')}</h1>
        <p className="mt-1 text-sm text-slate-500">{t('admin.usersPage.subtitle')}</p>
      </header>

      <section className="grid grid-cols-1 gap-3 rounded-xl border border-slate-200 bg-white p-4 lg:grid-cols-3">
        <label className="text-sm font-medium text-slate-700">
          {t('admin.usersPage.filters.search')}
          <input
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={searchValue}
            onChange={(event) => setSearchValue(event.target.value)}
            placeholder={t('admin.usersPage.filters.searchPlaceholder')}
            aria-label={t('admin.usersPage.filters.search')}
          />
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.usersPage.filters.role')}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={roleFilter}
            onChange={(event) => setRoleFilter(event.target.value)}
            aria-label={t('admin.usersPage.filters.role')}
          >
            {roleOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm font-medium text-slate-700">
          {t('admin.usersPage.filters.status')}
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value)}
            aria-label={t('admin.usersPage.filters.status')}
          >
            {statusOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
      </section>

      <section className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
        <table className="min-w-full divide-y divide-slate-200">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.usersPage.table.user')}
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.usersPage.table.status')}
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.usersPage.table.roles')}
              </th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">
                {t('admin.usersPage.table.actions')}
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {users.map((user) => {
              const roleLabels = getRoleLabels(user)
              const isEditing = editingRolesUserId === user.id
              return (
                <Fragment key={user.id}>
                  <tr className={isEditing ? 'bg-rose-50/40' : undefined}>
                    <td className="px-4 py-3">
                      <p className="font-medium text-slate-900">{user.fullName}</p>
                      <p className="text-sm text-slate-500">{user.email}</p>
                    </td>
                    <td className="px-4 py-3 text-sm text-slate-700">
                      {t(`admin.usersPage.status.${user.status.toLowerCase()}`)}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1.5">
                        {roleLabels.length ? (
                          roleLabels.map((label) => (
                            <span
                              key={`${user.id}-${label}`}
                              className="inline-flex rounded-full border border-slate-200 bg-slate-50 px-2 py-0.5 text-xs font-medium text-slate-700"
                            >
                              {label}
                            </span>
                          ))
                        ) : (
                          <span className="text-sm text-slate-500">{t('admin.usersPage.roles.none')}</span>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        type="button"
                        className="cursor-pointer rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-700 transition-colors hover:bg-slate-50"
                        onClick={() => openRoleEditor(user.id)}
                        aria-expanded={isEditing}
                      >
                        {isEditing
                          ? t('admin.usersPage.actions.closeRoles')
                          : t('admin.usersPage.actions.manageRoles')}
                      </button>
                    </td>
                  </tr>
                  {isEditing ? (
                    <tr>
                      <td colSpan={4} className="border-t border-rose-100 bg-rose-50/30 px-4 py-4">
                        {roleActionError ? (
                          <p className="mb-3 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
                            {roleActionError}
                          </p>
                        ) : null}
                        <AdminAssignListbox
                          assignedIds={user.roleIds ?? []}
                          items={roleListItems}
                          assignedTitle={t('admin.usersPage.roles.assigned')}
                          availableTitle={t('admin.usersPage.roles.add')}
                          emptyAssigned={t('admin.usersPage.roles.emptyAssigned')}
                          emptyAvailable={t('admin.usersPage.roles.emptyAvailable')}
                          searchPlaceholder={t('admin.usersPage.roles.search')}
                          isBusy={isRoleMutating}
                          onAssign={(roleId) => assignRole(user.id, roleId)}
                          onRemove={(roleId) => removeRole(user.id, roleId)}
                        />
                        <div className="mt-3 flex justify-end">
                          <button
                            type="button"
                            className="cursor-pointer rounded-md border border-slate-300 px-3 py-1.5 text-sm text-slate-700 hover:bg-white"
                            onClick={closeRoleEditor}
                          >
                            {t('common.close')}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ) : null}
                </Fragment>
              )
            })}
            {!users.length ? (
              <tr>
                <td colSpan={4} className="px-4 py-8 text-center text-sm text-slate-500">
                  {t('admin.usersPage.messages.noResults')}
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </section>
    </div>
  )
}
