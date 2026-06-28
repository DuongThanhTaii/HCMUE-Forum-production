import { createContext } from 'react'

export type AuthContextValue = {
  isAuthenticated: boolean
  userId: string | null
  requireAuth: (onUnauthenticated?: () => void) => boolean
}

export const authContext = createContext<AuthContextValue | undefined>(undefined)
