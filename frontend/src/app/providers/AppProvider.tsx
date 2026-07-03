import type { ReactNode } from 'react'
import { Provider } from 'react-redux'
import { AuthProvider } from '@features/auth/context/AuthContext'
import { store } from '../store'

type AppProviderProps = {
  children: ReactNode
}

import { Toaster } from 'react-hot-toast'

export const AppProvider = ({ children }: AppProviderProps) => {
  return (
    <Provider store={store}>
      <AuthProvider>
        {children}
        <Toaster position="top-right" />
      </AuthProvider>
    </Provider>
  )
}
