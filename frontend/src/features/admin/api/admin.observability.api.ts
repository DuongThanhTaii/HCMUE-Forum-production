import { baseApi } from '@shared/lib/api/baseApi'
import type {
  AuditLogDto,
  AuditLogsFilterParams,
  EndpointToggleDto,
  MaintenanceModeDto,
  SetMaintenanceModeRequest,
  SetEndpointToggleRequest,
  ThreadChannelDto,
  UpsertThreadChannelRequest,
  UserActionLogsFilterParams,
  UserActionLogsResponse,
} from '../types/admin.types'
import { unwrapApiData, unwrapApiList } from './admin.api'

type QueryPrimitive = string | number | boolean
type QueryParams = Record<string, QueryPrimitive>

function compactParams(input: Record<string, QueryPrimitive | undefined>): QueryParams {
  const entries = Object.entries(input).filter(([, value]) => value !== undefined) as [string, QueryPrimitive][]
  return Object.fromEntries(entries)
}

export function getTogglesPath(): string {
  return '/api/v1/admin/authorization/toggles'
}

export function getTogglePath(endpointKey: string): string {
  return `${getTogglesPath()}/${encodeURIComponent(endpointKey)}`
}

export function buildSetToggleRequest(endpointKey: string, body: SetEndpointToggleRequest) {
  return {
    url: getTogglePath(endpointKey),
    method: 'PUT' as const,
    body,
  }
}

export function getAuditLogsPath(): string {
  return '/api/v1/admin/authorization/audit-logs'
}

export function getMaintenanceModePath(): string {
  return '/api/v1/admin/authorization/maintenance-mode'
}

export function getAdminThreadChannelsPath(): string {
  return '/api/v1/thread-channels/admin'
}

export function buildAuditLogsParams(params?: AuditLogsFilterParams): QueryParams {
  return compactParams({
    userId: params?.userId,
    endpointKey: params?.endpointKey,
    isSuccess: params?.isSuccess,
    fromUtc: params?.fromUtc,
    toUtc: params?.toUtc,
    take: params?.take,
  })
}

export function getUserActionLogsPath(): string {
  return '/api/v1/admin/observability/user-actions'
}

export function buildUserActionLogsParams(params?: UserActionLogsFilterParams): QueryParams {
  return compactParams({
    actorUserId: params?.actorUserId,
    correlationId: params?.correlationId,
    traceId: params?.traceId,
    method: params?.method,
    pathContains: params?.pathContains,
    minStatusCode: params?.minStatusCode,
    maxStatusCode: params?.maxStatusCode,
    fromUtc: params?.fromUtc,
    toUtc: params?.toUtc,
    viewType: params?.viewType,
    page: params?.page,
    pageSize: params?.pageSize,
  })
}

export const adminObservabilityApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getToggles: builder.query<EndpointToggleDto[], void>({
      query: () => getTogglesPath(),
      transformResponse: (response: unknown) => unwrapApiList<EndpointToggleDto>(response),
      providesTags: (result) =>
        result?.length
          ? [
              ...result.map((toggle) => ({ type: 'AdminToggle' as const, id: toggle.endpointKey })),
              { type: 'AdminToggle' as const, id: 'LIST' },
            ]
          : [{ type: 'AdminToggle' as const, id: 'LIST' }],
    }),

    getToggle: builder.query<EndpointToggleDto, string>({
      query: (endpointKey) => getTogglePath(endpointKey),
      transformResponse: (response: unknown) => {
        const toggle = unwrapApiData<EndpointToggleDto>(response)
        if (!toggle) throw new Error('MISSING_TOGGLE')
        return toggle
      },
      providesTags: (_result, _error, endpointKey) => [{ type: 'AdminToggle', id: endpointKey }],
    }),

    getMaintenanceMode: builder.query<MaintenanceModeDto, void>({
      query: () => getMaintenanceModePath(),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<MaintenanceModeDto>(response)
        if (!payload) throw new Error('MISSING_MAINTENANCE_MODE')
        return payload
      },
      providesTags: [{ type: 'MaintenanceMode', id: 'CURRENT' }],
    }),

    setMaintenanceMode: builder.mutation<MaintenanceModeDto, SetMaintenanceModeRequest>({
      query: (body) => ({
        url: getMaintenanceModePath(),
        method: 'PUT',
        body,
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<MaintenanceModeDto>(response)
        if (!payload) throw new Error('MISSING_MAINTENANCE_MODE')
        return payload
      },
      invalidatesTags: [
        { type: 'MaintenanceMode', id: 'CURRENT' },
        { type: 'AdminToggle', id: 'LIST' },
      ],
    }),

    setToggle: builder.mutation<EndpointToggleDto, { endpointKey: string; body: SetEndpointToggleRequest }>({
      query: ({ endpointKey, body }) => buildSetToggleRequest(endpointKey, body),
      transformResponse: (response: unknown) => {
        const toggle = unwrapApiData<EndpointToggleDto>(response)
        if (!toggle) throw new Error('MISSING_TOGGLE')
        return toggle
      },
      invalidatesTags: (_result, _error, arg) => [
        { type: 'AdminToggle', id: arg.endpointKey },
        { type: 'AdminToggle', id: 'LIST' },
      ],
    }),

    getAdminThreadChannels: builder.query<ThreadChannelDto[], void>({
      query: () => getAdminThreadChannelsPath(),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<{ channels?: ThreadChannelDto[] }>(response)
        return payload?.channels ?? []
      },
      providesTags: (result) =>
        result?.length
          ? [
              ...result.map((item) => ({ type: 'AdminThreadChannel' as const, id: item.id })),
              { type: 'AdminThreadChannel' as const, id: 'LIST' },
            ]
          : [{ type: 'AdminThreadChannel' as const, id: 'LIST' }],
    }),

    createThreadChannel: builder.mutation<ThreadChannelDto, UpsertThreadChannelRequest>({
      query: (body) => ({
        url: '/api/v1/thread-channels',
        method: 'POST',
        body,
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<ThreadChannelDto>(response)
        if (!payload) throw new Error('MISSING_THREAD_CHANNEL')
        return payload
      },
      invalidatesTags: [{ type: 'AdminThreadChannel', id: 'LIST' }],
    }),

    updateThreadChannel: builder.mutation<ThreadChannelDto, { id: string; body: UpsertThreadChannelRequest }>({
      query: ({ id, body }) => ({
        url: `/api/v1/thread-channels/${id}`,
        method: 'PUT',
        body,
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<ThreadChannelDto>(response)
        if (!payload) throw new Error('MISSING_THREAD_CHANNEL')
        return payload
      },
      invalidatesTags: (_result, _error, arg) => [
        { type: 'AdminThreadChannel', id: arg.id },
        { type: 'AdminThreadChannel', id: 'LIST' },
      ],
    }),

    getAuditLogs: builder.query<AuditLogDto[], AuditLogsFilterParams | undefined>({
      query: (params) => ({
        url: getAuditLogsPath(),
        params: buildAuditLogsParams(params),
      }),
      transformResponse: (response: unknown) => unwrapApiList<AuditLogDto>(response),
    }),

    getUserActionLogs: builder.query<UserActionLogsResponse, UserActionLogsFilterParams | undefined>({
      query: (params) => ({
        url: getUserActionLogsPath(),
        params: buildUserActionLogsParams(params),
      }),
      transformResponse: (response: unknown) => {
        const payload = unwrapApiData<UserActionLogsResponse>(response)
        if (!payload) throw new Error('MISSING_USER_ACTION_LOGS')
        return {
          ...payload,
          items: payload.items.map((item) => ({
            ...item,
            requestHeadersJson: item.requestHeadersJson ?? '{}',
            requestBodyTruncated: item.requestBodyTruncated ?? false,
            responseHeadersJson: item.responseHeadersJson ?? '{}',
            responseBodyTruncated: item.responseBodyTruncated ?? false,
          })),
        }
      },
    }),
  }),
})

export const {
  useGetTogglesQuery,
  useGetToggleQuery,
  useGetMaintenanceModeQuery,
  useSetMaintenanceModeMutation,
  useSetToggleMutation,
  useGetAdminThreadChannelsQuery,
  useCreateThreadChannelMutation,
  useUpdateThreadChannelMutation,
  useGetAuditLogsQuery,
  useGetUserActionLogsQuery,
} = adminObservabilityApi
