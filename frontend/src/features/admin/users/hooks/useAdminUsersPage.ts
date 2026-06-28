import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  useAssignRoleToUserMutation,
  useGetRolesQuery,
  useGetUsersQuery,
  useRemoveRoleFromUserMutation,
} from '../../api/admin.api'
import type { UserDto, UserStatus } from '../../types/admin.types'

type FilterOption = { value: string; label: string }

export function useAdminUsersPage() {
  const { t } = useTranslation()
  const { data: usersData, isLoading: isUsersLoading, isError: isUsersError } = useGetUsersQuery()
  const { data: rolesData, isLoading: isRolesLoading, isError: isRolesError } = useGetRolesQuery()
  const [assignRoleToUser, { isLoading: isAssigningRole }] = useAssignRoleToUserMutation()
  const [removeRoleFromUser, { isLoading: isRemovingRole }] = useRemoveRoleFromUserMutation()

  const [searchValue, setSearchValue] = useState('')
  const [roleFilter, setRoleFilter] = useState('all')
  const [statusFilter, setStatusFilter] = useState<'all' | UserStatus>('all')
  const [editingRolesUserId, setEditingRolesUserId] = useState<string | null>(null)
  const [roleActionError, setRoleActionError] = useState<string | null>(null)

  const users = useMemo(() => usersData ?? [], [usersData])
  const roles = useMemo(() => rolesData ?? [], [rolesData])
  const roleNameById = useMemo(() => new Map(roles.map((r) => [r.id, r.name])), [roles])
  const normalizedSearch = searchValue.trim().toLowerCase()

  const filteredUsers = useMemo(
    () =>
      users.filter((user) => {
        const bySearch =
          !normalizedSearch ||
          user.fullName.toLowerCase().includes(normalizedSearch) ||
          user.email.toLowerCase().includes(normalizedSearch)
        const byStatus = statusFilter === 'all' || user.status === statusFilter
        const userRoleIds = user.roleIds ?? []
        const byRole =
          roleFilter === 'all' || userRoleIds.some((roleId) => roleId === roleFilter)
        return bySearch && byStatus && byRole
      }),
    [users, normalizedSearch, statusFilter, roleFilter],
  )

  const roleOptions: FilterOption[] = useMemo(
    () => [
      { value: 'all', label: t('admin.usersPage.filters.allRoles') },
      ...roles.map((r) => ({ value: r.id, label: r.name })),
    ],
    [roles, t],
  )

  const statusOptions: FilterOption[] = useMemo(
    () => [
      { value: 'all', label: t('admin.usersPage.filters.allStatuses') },
      { value: 'Active', label: t('admin.usersPage.status.active') },
      { value: 'Inactive', label: t('admin.usersPage.status.inactive') },
      { value: 'Banned', label: t('admin.usersPage.status.banned') },
    ],
    [t],
  )

  const roleListItems = useMemo(
    () => roles.map((r) => ({ id: r.id, label: r.name, hint: r.description || undefined })),
    [roles],
  )

  const editingUser = useMemo(
    () => users.find((user) => user.id === editingRolesUserId) ?? null,
    [users, editingRolesUserId],
  )

  const openRoleEditor = (userId: string) => {
    setRoleActionError(null)
    setEditingRolesUserId((current) => (current === userId ? null : userId))
  }

  const closeRoleEditor = () => {
    setEditingRolesUserId(null)
    setRoleActionError(null)
  }

  const assignRole = async (userId: string, roleId: string) => {
    setRoleActionError(null)
    try {
      await assignRoleToUser({ userId, body: { roleId } }).unwrap()
    } catch {
      setRoleActionError(t('admin.usersPage.messages.assignRoleFailed'))
    }
  }

  const removeRole = async (userId: string, roleId: string) => {
    setRoleActionError(null)
    const user = users.find((u) => u.id === userId)
    const roleCount = user?.roleIds?.length ?? 0
    if (roleCount <= 1) {
      setRoleActionError(t('admin.usersPage.messages.cannotRemoveLastRole'))
      return
    }
    try {
      await removeRoleFromUser({ userId, roleId }).unwrap()
    } catch {
      setRoleActionError(t('admin.usersPage.messages.removeRoleFailed'))
    }
  }

  const getRoleLabels = (user: UserDto): string[] =>
    (user.roleIds ?? []).map((id) => roleNameById.get(id) ?? id)

  return {
    t,
    users: filteredUsers,
    roles,
    roleOptions,
    statusOptions,
    roleListItems,
    roleNameById,
    searchValue,
    roleFilter,
    statusFilter,
    editingRolesUserId,
    editingUser,
    roleActionError,
    isRoleMutating: isAssigningRole || isRemovingRole,
    isLoading: isUsersLoading || isRolesLoading,
    isError: isUsersError || isRolesError,
    setSearchValue,
    setRoleFilter,
    setStatusFilter: (value: string) => setStatusFilter(value as 'all' | UserStatus),
    openRoleEditor,
    closeRoleEditor,
    assignRole,
    removeRole,
    getRoleLabels,
  }
}

export type AdminUsersPageHook = ReturnType<typeof useAdminUsersPage>
