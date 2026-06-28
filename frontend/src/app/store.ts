import { configureStore } from '@reduxjs/toolkit'
import '@features/admin/api/admin.api'
import '@features/chat/api/chat.api'
import { authReducer } from '@features/auth/model/auth.slice'
import { loadAuthState, saveAuthState } from '@features/auth/model/auth.storage'
import { baseApi } from '@shared/lib/api/baseApi'

const preloadedAuthState = loadAuthState()

export const store = configureStore({
  reducer: {
    auth: authReducer,
    [baseApi.reducerPath]: baseApi.reducer,
  },
  preloadedState: preloadedAuthState
    ? {
        auth: preloadedAuthState,
      }
    : undefined,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(baseApi.middleware),
})

store.subscribe(() => {
  saveAuthState(store.getState().auth)
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
