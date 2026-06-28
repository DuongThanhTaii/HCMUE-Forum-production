import type { ReactNode } from 'react'
import { selectAuth, selectIsAuthenticated } from '@features/auth/model/auth.slice'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { authContext, type AuthContextValue } from './AuthContextStore'

type AuthProviderProps = {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const isAuthenticated = useAppSelector(selectIsAuthenticated)
  const auth = useAppSelector(selectAuth)

  const value: AuthContextValue = {
    isAuthenticated,
    userId: auth.user?.id ?? null,
    requireAuth(onUnauthenticated) {
      if (isAuthenticated) {
        return true
      }

      onUnauthenticated?.()
      return false
    },
  }

  return <authContext.Provider value={value}>{children}</authContext.Provider>
}
