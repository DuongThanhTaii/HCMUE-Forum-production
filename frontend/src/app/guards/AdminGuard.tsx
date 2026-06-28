import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@features/auth/context/useAuth'
import { selectUserRole } from '@features/auth/model/auth.slice'
import { useAppSelector } from '@shared/hooks/useAppSelector'

const AUTH_BYPASS_IN_DEV =
  import.meta.env.DEV && import.meta.env.VITE_DEV_BYPASS_AUTH !== 'false'

const normalizeRole = (role: unknown): string | null => {
  if (typeof role !== 'string') {
    return null
  }

  const normalized = role.trim().toLowerCase()
  return normalized.length > 0 ? normalized : null
}

export function AdminGuard() {
  const { isAuthenticated } = useAuth()
  const userRoles = useAppSelector(selectUserRole)

  if (AUTH_BYPASS_IN_DEV) {
    return <Outlet />
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  const hasAdminRole = userRoles.some((role: unknown) => normalizeRole(role) === 'admin')
  if (!hasAdminRole) {
    return <Navigate to="/home" replace />
  }

  return <Outlet />
}
