import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { skipToken } from '@reduxjs/toolkit/query'
import {
  useGetGroupOverridesQuery,
  useGetPermissionsQuery,
  useGetUserGroupsQuery,
  useGetUserOverridesQuery,
  useGetUsersQuery,
  useRevokeGroupOverrideMutation,
  useRevokeUserOverrideMutation,
  useUpsertGroupOverrideMutation,
  useUpsertUserOverrideMutation,
} from '../../api/admin.api'
import type { OverrideEffect, RevokePermissionOverrideRequest, UpsertPermissionOverrideRequest } from '../../types/admin.types'

type OverrideTab = 'users' | 'groups'

export function useAdminOverridesPage() {
  const { t } = useTranslation()
  const [activeTab, setActiveTab] = useState<OverrideTab>('users')
  const [searchValue, setSearchValue] = useState('')
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null)
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null)

  const { data: usersData, isLoading: isUsersLoading, isError: isUsersError } = useGetUsersQuery()
  const { data: groupsData, isLoading: isGroupsLoading, isError: isGroupsError } = useGetUserGroupsQuery()
  const { data: permissionsData, isLoading: isPermissionsLoading, isError: isPermissionsError } = useGetPermissionsQuery()
  const { data: overridesData, isLoading: isOverridesLoading, isError: isOverridesError } = useGetUserOverridesQuery(
    selectedUserId ?? skipToken,
  )
  const { data: groupOverridesData, isLoading: isGroupOverridesLoading, isError: isGroupOverridesError } = useGetGroupOverridesQuery(
    selectedGroupId ?? skipToken,
  )
  const [upsertUserOverride, { isLoading: isUpserting }] = useUpsertUserOverrideMutation()
  const [revokeUserOverride, { isLoading: isRevoking }] = useRevokeUserOverrideMutation()
  const [upsertGroupOverride, { isLoading: isUpsertingGroup }] = useUpsertGroupOverrideMutation()
  const [revokeGroupOverride, { isLoading: isRevokingGroup }] = useRevokeGroupOverrideMutation()

  const users = useMemo(() => usersData ?? [], [usersData])
  const groups = useMemo(() => groupsData ?? [], [groupsData])
  const permissions = useMemo(() => permissionsData ?? [], [permissionsData])
  const overrides = useMemo(() => overridesData ?? [], [overridesData])
  const groupOverrides = useMemo(() => groupOverridesData ?? [], [groupOverridesData])
  const normalizedSearch = searchValue.trim().toLowerCase()

  const filteredUsers = useMemo(
    () =>
      users.filter((user) => {
        if (!normalizedSearch) return true
        return user.fullName.toLowerCase().includes(normalizedSearch) || user.email.toLowerCase().includes(normalizedSearch)
      }),
    [users, normalizedSearch],
  )

  const selectedUser = useMemo(() => users.find((user) => user.id === selectedUserId) ?? null, [users, selectedUserId])
  const selectedGroup = useMemo(() => groups.find((group) => group.id === selectedGroupId) ?? null, [groups, selectedGroupId])

  const submitUserOverride = async (input: {
    permissionId: string
    scopeType: string
    scopeValue: string | null
    effect: OverrideEffect
    reason: string
    expiresAtUtc: string
  }) => {
    if (!selectedUserId) return
    const payload: UpsertPermissionOverrideRequest = {
      permissionId: input.permissionId,
      scopeType: input.scopeType,
      scopeValue: input.scopeValue || null,
      effect: input.effect,
      reason: input.reason.trim() || null,
      expiresAtUtc: input.expiresAtUtc.trim() || null,
    }
    await upsertUserOverride({ userId: selectedUserId, body: payload }).unwrap()
  }

  const revokeOverride = async (input: RevokePermissionOverrideRequest) => {
    if (!selectedUserId) return
    await revokeUserOverride({ userId: selectedUserId, query: input }).unwrap()
  }

  const submitGroupOverride = async (input: {
    permissionId: string
    scopeType: string
    scopeValue: string | null
    effect: OverrideEffect
    reason: string
    expiresAtUtc: string
  }) => {
    if (!selectedGroupId) return
    const payload: UpsertPermissionOverrideRequest = {
      permissionId: input.permissionId,
      scopeType: input.scopeType,
      scopeValue: input.scopeValue || null,
      effect: input.effect,
      reason: input.reason.trim() || null,
      expiresAtUtc: input.expiresAtUtc.trim() || null,
    }
    await upsertGroupOverride({ groupId: selectedGroupId, body: payload }).unwrap()
  }

  const revokeGroup = async (input: RevokePermissionOverrideRequest) => {
    if (!selectedGroupId) return
    await revokeGroupOverride({ groupId: selectedGroupId, query: input }).unwrap()
  }

  return {
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
    isUsersLoading,
    isPermissionsLoading,
    isOverridesLoading,
    isLoading: isUsersLoading || isPermissionsLoading || (Boolean(selectedUserId) && isOverridesLoading),
    isError: isUsersError || isPermissionsError || isOverridesError || isGroupsError || isGroupOverridesError,
    isMutating: isUpserting || isRevoking || isUpsertingGroup || isRevokingGroup,
    submitUserOverride,
    revokeOverride,
    submitGroupOverride,
    revokeGroupOverride: revokeGroup,
    isGroupSourceAvailable: true,
    isGroupsLoading,
    isGroupOverridesLoading,
  }
}
