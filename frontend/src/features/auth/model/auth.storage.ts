import type { AuthState } from './auth.slice'

const AUTH_STORAGE_KEY = 'unihub.auth.v1'

type PersistedAuthState = Pick<AuthState, 'user' | 'accessToken' | 'refreshToken' | 'isAuthenticated'>

export function loadAuthState(): PersistedAuthState | undefined {
  if (typeof window === 'undefined') return undefined
  try {
    const raw = window.localStorage.getItem(AUTH_STORAGE_KEY)
    if (!raw) return undefined
    const parsed = JSON.parse(raw) as PersistedAuthState
    if (!parsed?.accessToken || !parsed?.user?.id) return undefined
    return parsed
  } catch {
    return undefined
  }
}

export function saveAuthState(state: AuthState): void {
  if (typeof window === 'undefined') return
  try {
    if (!state.isAuthenticated || !state.accessToken || !state.user?.id) {
      window.localStorage.removeItem(AUTH_STORAGE_KEY)
      return
    }
    const payload: PersistedAuthState = {
      user: state.user,
      accessToken: state.accessToken,
      refreshToken: state.refreshToken,
      isAuthenticated: state.isAuthenticated,
    }
    window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(payload))
  } catch {
    // ignore storage failures
  }
}
