import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type {
  BaseQueryFn,
  FetchArgs,
  FetchBaseQueryError,
} from '@reduxjs/toolkit/query'
import { logout, setTokensFromRefresh, setUserRoles } from '@features/auth/model/auth.slice'
import { parseRolesFromAccessToken } from '@features/auth/lib/token'
import type { RootState } from '../../../app/store'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5034'

type ApiEnvelope<T> = {
  success?: boolean
  message?: string
  data?: T
}

type RefreshTokenData = {
  accessToken?: string
  refreshToken?: string
}

type RefreshResponse = {
  accessToken: string
  refreshToken: string
}
let refreshPromise: Promise<RefreshResponse | null> | null = null
let isRedirectingToMaintenance = false

type MaintenanceErrorPayload = {
  title?: string
  extensions?: {
    reasonCode?: string
  }
}

const isMaintenanceModeError = (error: FetchBaseQueryError | undefined): boolean => {
  if (!error || error.status !== 503) return false
  const payload = (error.data ?? {}) as MaintenanceErrorPayload
  const reasonCode = payload.extensions?.reasonCode
  return reasonCode === 'MaintenanceModeEnabled' || payload.title === 'Maintenance Mode'
}

const redirectToMaintenanceIfNeeded = (error: FetchBaseQueryError | undefined): void => {
  if (!isMaintenanceModeError(error)) return
  if (typeof window === 'undefined') return
  if (window.location.pathname === '/maintenance') return
  // Keep admin pages accessible so admins can disable maintenance mode.
  if (window.location.pathname.startsWith('/admin')) return
  if (isRedirectingToMaintenance) return
  isRedirectingToMaintenance = true
  window.location.replace('/maintenance')
}

const isAzureAccessToken = (token: string | null | undefined): boolean => {
  if (!token) return false
  try {
    const parts = token.split('.')
    if (parts.length < 2) return false
    const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'))) as Record<string, unknown>
    const iss = typeof payload.iss === 'string' ? payload.iss.toLowerCase() : ''
    return iss.includes('login.microsoftonline.com') || iss.includes('sts.windows.net')
  } catch {
    return false
  }
}

const rawBaseQuery = fetchBaseQuery({
  baseUrl: API_URL,
  prepareHeaders: (headers, { getState, arg }) => {
    const accessToken = (getState() as RootState).auth?.accessToken

    if (accessToken) {
      headers.set('authorization', `Bearer ${accessToken}`)
    }

    const isFormData =
      typeof arg === 'object' &&
      arg !== null &&
      'body' in arg &&
      arg.body instanceof FormData
    if (!isFormData) {
      headers.set('content-type', 'application/json')
    }
    return headers
  },
})

export const baseQueryWithReauth: BaseQueryFn<
  string | FetchArgs,
  unknown,
  FetchBaseQueryError
> = async (args, api, extraOptions) => {
  let result = await rawBaseQuery(args, api, extraOptions)
  redirectToMaintenanceIfNeeded(result.error)

  if (result.error?.status !== 401) {
    return result
  }

  const accessToken = (api.getState() as RootState).auth?.accessToken
  const refreshToken = (api.getState() as RootState).auth?.refreshToken
  if (!refreshToken) {
    if (isAzureAccessToken(accessToken)) {
      // Azure flow does not use local refresh token endpoint.
      // Keep current session and let caller handle unauthorized responses.
      return result
    }
    api.dispatch(logout())
    window.location.href = '/login'
    return result
  }

  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshResult = await rawBaseQuery(
        { url: '/api/v1/auth/refresh', method: 'POST', body: { refreshToken } },
        api,
        extraOptions
      )
      const envelope = refreshResult.data as ApiEnvelope<RefreshTokenData> | undefined
      const payload = envelope?.data
      const accessToken = payload?.accessToken?.trim()
      const nextRefreshToken = payload?.refreshToken?.trim()

      if (!accessToken || !nextRefreshToken) {
        return null
      }

      return { accessToken, refreshToken: nextRefreshToken }
    })().finally(() => {
      refreshPromise = null
    })
  }

  const nextAuth = await refreshPromise
  if (nextAuth) {
    api.dispatch(setTokensFromRefresh(nextAuth))
    const nextRoles = parseRolesFromAccessToken(nextAuth.accessToken)
    api.dispatch(setUserRoles(nextRoles))
    result = await rawBaseQuery(args, api, extraOptions)
    redirectToMaintenanceIfNeeded(result.error)
  } else {
    api.dispatch(logout())
    window.location.href = '/login'
  }

  return result
}
