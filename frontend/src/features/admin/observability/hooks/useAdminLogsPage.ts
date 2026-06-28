import { useEffect, useMemo, useState } from 'react'
import { skipToken } from '@reduxjs/toolkit/query'
import { useTranslation } from 'react-i18next'
import {
  useGetAuditLogsQuery,
  useGetMaintenanceModeQuery,
  useGetTogglesQuery,
  useGetUserActionLogsQuery,
  useSetMaintenanceModeMutation,
  useSetToggleMutation,
} from '../../api/admin.observability.api'
import type { SetEndpointToggleRequest, UserActionLogsViewType } from '../../types/admin.types'
import { getActionLogPathFilter } from '../lib/action-log-features'

export type ActionLogOutcomeFilter = 'all' | 'success' | 'client_error' | 'server_error'

const NO_ACTION_LOG_STATUS_FILTER: Record<string, never> = {}

export function useAdminLogsPage() {
  const { t } = useTranslation()
  const { data: togglesData, isLoading: isTogglesLoading, isError: isTogglesError } = useGetTogglesQuery()
  const {
    data: maintenanceModeData,
    isLoading: isMaintenanceModeLoading,
    isError: isMaintenanceModeError,
  } = useGetMaintenanceModeQuery()
  const [setToggleMutation, { isLoading: isSetToggleLoading }] = useSetToggleMutation()
  const [setMaintenanceModeMutation, { isLoading: isSetMaintenanceModeLoading }] = useSetMaintenanceModeMutation()

  const [auditUserId, setAuditUserId] = useState('')
  const [auditEndpointKey, setAuditEndpointKey] = useState('')
  const [auditResultFilter, setAuditResultFilter] = useState<'all' | 'success' | 'failure'>('all')
  const [auditTake, setAuditTake] = useState(50)
  const [auditFromUtc, setAuditFromUtc] = useState('')
  const [auditToUtc, setAuditToUtc] = useState('')

  const auditParams = useMemo(
    () => ({
      userId: auditUserId.trim() || undefined,
      endpointKey: auditEndpointKey.trim() || undefined,
      isSuccess: auditResultFilter === 'all' ? undefined : auditResultFilter === 'success',
      take: auditTake,
      fromUtc: auditFromUtc.trim() || undefined,
      toUtc: auditToUtc.trim() || undefined,
    }),
    [auditUserId, auditEndpointKey, auditResultFilter, auditTake, auditFromUtc, auditToUtc],
  )

  const {
    data: auditLogsData,
    isLoading: isAuditLogsLoading,
    isError: isAuditLogsError,
  } = useGetAuditLogsQuery(auditUserId.trim() ? auditParams : skipToken, {
    pollingInterval: 5000,
  })

  const [actionViewType, setActionViewTypeState] = useState<UserActionLogsViewType>('Developer')
  const [actionPage, setActionPage] = useState(1)
  const [actionPageSize, setActionPageSize] = useState(100)
  const [actionActorUserId, setActionActorUserId] = useState('')
  const [actionFeatureId, setActionFeatureId] = useState('all')
  const [actionOutcome, setActionOutcome] = useState<ActionLogOutcomeFilter>('all')
  const [actionCorrelationId, setActionCorrelationId] = useState('')
  const [actionTraceId, setActionTraceId] = useState('')
  const [actionFromUtc, setActionFromUtc] = useState('')
  const [actionToUtc, setActionToUtc] = useState('')

  const pathContainsFilter = getActionLogPathFilter(actionFeatureId)

  const statusRange = useMemo((): { minStatusCode?: number; maxStatusCode?: number } => {
    switch (actionOutcome) {
      case 'success':
        return { minStatusCode: 200, maxStatusCode: 399 }
      case 'client_error':
        return { minStatusCode: 400, maxStatusCode: 499 }
      case 'server_error':
        return { minStatusCode: 500, maxStatusCode: 599 }
      default:
        return NO_ACTION_LOG_STATUS_FILTER
    }
  }, [actionOutcome])

  const actionQueryEnabled = Boolean(
    actionActorUserId.trim() || actionCorrelationId.trim() || actionTraceId.trim(),
  )

  const actionParams = useMemo(
    () => ({
      viewType: actionViewType,
      page: actionPage,
      pageSize: actionPageSize,
      actorUserId: actionActorUserId.trim() || undefined,
      pathContains: pathContainsFilter,
      correlationId: actionCorrelationId.trim() || undefined,
      traceId: actionTraceId.trim() || undefined,
      fromUtc: actionFromUtc.trim() || undefined,
      toUtc: actionToUtc.trim() || undefined,
      ...(actionOutcome === 'all' ? {} : statusRange),
    }),
    [
      actionViewType,
      actionPage,
      actionPageSize,
      actionActorUserId,
      pathContainsFilter,
      actionCorrelationId,
      actionTraceId,
      actionFromUtc,
      actionToUtc,
      actionOutcome,
      statusRange,
    ],
  )

  useEffect(() => {
    setActionPage(1)
  }, [actionFeatureId, actionOutcome, actionActorUserId, actionCorrelationId, actionTraceId, actionViewType])

  const {
    data: actionLogsData,
    isLoading: isActionLogsLoading,
    isError: isActionLogsError,
  } = useGetUserActionLogsQuery(actionQueryEnabled ? actionParams : skipToken, {
    pollingInterval: 5000,
  })

  const setActionViewType = (viewType: UserActionLogsViewType) => {
    setActionViewTypeState(viewType)
    setActionPage(1)
  }

  const submitToggle = async (endpointKey: string, isEnabled: boolean, reason: string | null) => {
    const payload: SetEndpointToggleRequest = {
      isEnabled,
      reason: isEnabled ? null : reason,
    }
    await setToggleMutation({ endpointKey, body: payload }).unwrap()
  }

  const submitMaintenanceMode = async (isEnabled: boolean, reason: string | null) => {
    await setMaintenanceModeMutation({
      isEnabled,
      reason: isEnabled ? reason : null,
    }).unwrap()
  }

  return {
    t,
    toggles: togglesData ?? [],
    isTogglesLoading,
    isTogglesError,
    isSetToggleLoading,
    submitToggle,
    maintenanceMode: maintenanceModeData ?? null,
    isMaintenanceModeLoading,
    isMaintenanceModeError,
    isSetMaintenanceModeLoading,
    submitMaintenanceMode,

    auditLogs: auditLogsData ?? [],
    isAuditLogsLoading,
    isAuditLogsError,
    auditUserId,
    setAuditUserId,
    auditEndpointKey,
    setAuditEndpointKey,
    auditResultFilter,
    setAuditResultFilter,
    auditTake,
    setAuditTake,
    auditFromUtc,
    setAuditFromUtc,
    auditToUtc,
    setAuditToUtc,

    actionItems: actionLogsData?.items ?? [],
    actionTotal: actionLogsData?.total ?? 0,
    actionPage,
    setActionPage,
    actionPageSize,
    setActionPageSize,
    actionViewType,
    setActionViewType,
    availableActionViewTypes: actionLogsData?.availableViewTypes ?? ['Developer', 'Administrator'],
    isActionLogsLoading,
    isActionLogsError,
    actionActorUserId,
    setActionActorUserId,
    actionFeatureId,
    setActionFeatureId,
    actionOutcome,
    setActionOutcome,
    actionCorrelationId,
    setActionCorrelationId,
    actionTraceId,
    setActionTraceId,
    actionFromUtc,
    setActionFromUtc,
    actionToUtc,
    setActionToUtc,
    actionQueryEnabled,
  }
}
