import { useAdminRolesPage } from '../hooks/useAdminRolesPage'
import { CreateRoleModal } from './CreateRoleModal'
import { RoleList } from './RoleList'
import { RolePermissionGrid } from './RolePermissionGrid'

export function AdminRolesPage() {
  const {
    t,
    roles,
    permissions,
    selectedRoleId,
    selectedRole,
    isLoading,
    isError,
    isCreateModalOpen,
    isCreatingRole,
    isAssigningPermission,
    isRemovingPermission,
    openCreateModal,
    closeCreateModal,
    selectRole,
    createRole,
    isPermissionAssigned,
    togglePermission,
  } = useAdminRolesPage()

  if (isLoading) {
    return <div className="rounded-xl border border-slate-200 bg-white p-4">{t('common.loading')}</div>
  }

  if (isError) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">
        {t('admin.rolesPage.messages.loadError')}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <header className="flex items-center justify-between rounded-xl border border-slate-200 bg-white p-4">
        <div>
          <h1 className="text-lg font-semibold text-slate-900">
            {t('admin.rolesPage.title')}
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            {t('admin.rolesPage.subtitle')}
          </p>
        </div>
        <button
          type="button"
          className="rounded-md bg-rose-600 px-3 py-2 text-sm font-medium text-white hover:bg-rose-700"
          onClick={openCreateModal}
        >
          {t('admin.rolesPage.createRole')}
        </button>
      </header>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[280px_minmax(0,1fr)]">
        <section className="rounded-xl border border-slate-200 bg-white p-3">
          <RoleList
            roles={roles}
            selectedRoleId={selectedRoleId}
            onSelectRole={selectRole}
            totalPermissionsInSystem={permissions.length}
          />
        </section>
        <RolePermissionGrid
          permissions={permissions}
          selectedRole={selectedRole}
          noRoleText={t('admin.rolesPage.messages.noRoleSelected')}
          isPermissionAssigned={isPermissionAssigned}
          onTogglePermission={togglePermission}
          isBusy={isAssigningPermission || isRemovingPermission}
        />
      </div>

      <CreateRoleModal
        isOpen={isCreateModalOpen}
        isSubmitting={isCreatingRole}
        title={t('admin.rolesPage.createModal.title')}
        nameLabel={t('admin.rolesPage.createModal.name')}
        descriptionLabel={t('admin.rolesPage.createModal.description')}
        cancelLabel={t('common.cancel')}
        submitLabel={t('common.create')}
        onClose={closeCreateModal}
        onSubmit={createRole}
      />
    </div>
  )
}
