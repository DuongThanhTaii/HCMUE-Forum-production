import { createSlice, type PayloadAction } from '@reduxjs/toolkit'
import type { AuthPayload, User } from '@shared/types/auth'
import type { RootState } from '../../../app/store'

export interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
}

const initialState: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
}

type RefreshTokenPayload = {
  accessToken: string
  refreshToken: string
}

const hasValidAuth = (payload: { accessToken?: string | null; user?: User | null }) =>
  Boolean(payload.accessToken && payload.user?.id)

const applyAuthPayload = (state: AuthState, payload: AuthPayload) => {
  state.user = payload.user
  state.accessToken = payload.accessToken
  state.refreshToken = payload.refreshToken
  state.isAuthenticated = hasValidAuth(payload)
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setAuth(state, action: PayloadAction<AuthPayload>) {
      applyAuthPayload(state, action.payload)
    },
    setTokensFromRefresh(state, action: PayloadAction<RefreshTokenPayload>) {
      state.accessToken = action.payload.accessToken
      state.refreshToken = action.payload.refreshToken
      state.isAuthenticated = hasValidAuth({
        accessToken: state.accessToken,
        user: state.user,
      })
    },
    setUserRoles(state, action: PayloadAction<string[]>) {
      if (!state.user) {
        return
      }
      state.user.roles = action.payload
    },
    logout() {
      return initialState
    },
  },
})

export const { setAuth, setTokensFromRefresh, setUserRoles, logout } = authSlice.actions
export const authReducer = authSlice.reducer

export const selectAuth = (state: RootState) => state.auth
export const selectIsAuthenticated = (state: RootState) => state.auth.isAuthenticated
export const selectUserId = (state: RootState) => state.auth.user?.id ?? null
export const selectUserRole = (state: RootState) => state.auth.user?.roles ?? []
export const selectUser = (state: RootState) => state.auth.user

