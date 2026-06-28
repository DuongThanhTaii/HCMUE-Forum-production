import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'

import { useAdminOverridesPage } from '../hooks/useAdminOverridesPage'
import { GroupOverridesPanel } from './GroupOverridesPanel'
import { UserOverridesPanel } from './UserOverridesPanel'

export function AdminOverridesPage() {
  const {
    t,
    activeTab,
    setActiveTab,
    searchValue,
    setSearchValue,
    selectedUserId,
    setSelectedUserId,
    selectedUser,
    groups,
    selectedGroupId,
    setSelectedGroupId,
    selectedGroup,
    filteredUsers,
    permissions,
    overrides,
    groupOverrides,
    isOverridesLoading,
    isMutating,
    submitUserOverride,
    revokeOverride,
    submitGroupOverride,
    revokeGroupOverride,
    isError,
    isGroupSourceAvailable,
    isGroupOverridesLoading,
  } = useAdminOverridesPage()
  const location = useLocation()

  useEffect(() => {
    if (location.pathname.endsWith('/groups')) {
      setActiveTab('groups')
      return
    }
    if (location.pathname.endsWith('/users')) {
      setActiveTab('users')
    }
  }, [location.pathname, setActiveTab])

  if (isError) {
    return <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-rose-700">{t('admin.overridesPage.messages.loadError')}</div>
  }

  return (
    <div className="space-y-4">
      <header className="rounded-xl border border-slate-200 bg-white p-4">
        <h1 className="text-lg font-semibold text-slate-900">{t('admin.overridesPage.title')}</h1>
        <p className="mt-1 text-sm text-slate-500">
          {t('admin.overridesPage.subtitle')}
        </p>
      </header>

      <div className="inline-flex rounded-lg border border-slate-200 bg-white p-1">
        <button
          type="button"
          className={`rounded-md px-3 py-1.5 text-sm ${activeTab === 'users' ? 'bg-rose-100 text-rose-700' : 'text-slate-700'}`}
          onClick={() => setActiveTab('users')}
        >
          {t('admin.overrides.users')}
        </button>
        <button
          type="button"
          className={`rounded-md px-3 py-1.5 text-sm ${activeTab === 'groups' ? 'bg-rose-100 text-rose-700' : 'text-slate-700'}`}
          onClick={() => setActiveTab('groups')}
        >
          {t('admin.overrides.groups')}
        </button>
      </div>

      {activeTab === 'users' ? (
        <UserOverridesPanel
          searchValue={searchValue}
          onSearchValueChange={setSearchValue}
          users={filteredUsers}
          selectedUserId={selectedUserId}
          onSelectUser={setSelectedUserId}
          selectedUser={selectedUser}
          overrides={overrides}
          permissions={permissions}
          isLoading={isOverridesLoading}
          isMutating={isMutating}
          onSubmitOverride={submitUserOverride}
          onRevokeOverride={revokeOverride}
        />
      ) : (
        <GroupOverridesPanel
          isGroupSourceAvailable={isGroupSourceAvailable}
          groups={groups}
          selectedGroupId={selectedGroupId}
          onSelectGroup={setSelectedGroupId}
          selectedGroup={selectedGroup}
          overrides={groupOverrides}
          permissions={permissions}
          isLoading={isGroupOverridesLoading}
          isMutating={isMutating}
          onSubmitOverride={submitGroupOverride}
          onRevokeOverride={revokeGroupOverride}
        />
      )}
    </div>
  )
}
