import { Navigate, Outlet } from 'react-router-dom'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserRole } from '@features/auth/model/auth.slice'
import { useAuth } from '@features/auth/context/useAuth'
import { AUTH_BYPASS_IN_DEV } from './authDevBypass'

const normalizeRole = (role: unknown): string | null => {
  if (typeof role !== 'string') {
    return null
  }

  const normalized = role.trim().toLowerCase()
  return normalized.length > 0 ? normalized : null
}

export function RequireAuth() {
  const { isAuthenticated } = useAuth()

  if (AUTH_BYPASS_IN_DEV) {
    return <Outlet />
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }
  return <Outlet />
}

export function RequireRole({ roles }: { roles: string[] }) {
  const { isAuthenticated } = useAuth()
  const userRoles = useAppSelector(selectUserRole)

  if (AUTH_BYPASS_IN_DEV) {
    return <Outlet />
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  const allowedRoles = new Set(
    roles
      .map(normalizeRole)
      .filter((role): role is string => role !== null)
  )
  const hasRole = userRoles.some((role: unknown) => {
    const normalizedRole = normalizeRole(role)
    return normalizedRole !== null && allowedRoles.has(normalizedRole)
  })
  if (!hasRole) {
    return <Navigate to="/home" replace />
  }
  return <Outlet />
}
