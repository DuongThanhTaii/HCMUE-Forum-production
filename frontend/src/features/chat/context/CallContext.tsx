import { createContext, useContext } from 'react'
import { useWebRtcCall } from '../hooks/useWebRtcCall'

type CallContextValue = ReturnType<typeof useWebRtcCall>

const CallContext = createContext<CallContextValue | undefined>(undefined)

export function CallProvider({ children }: { children: React.ReactNode }) {
  const call = useWebRtcCall()

  return (
    <CallContext.Provider value={call}>
      {children}
    </CallContext.Provider>
  )
}

export function useCallContext() {
  const ctx = useContext(CallContext)
  if (!ctx) {
    throw new Error('useCallContext must be used within CallProvider')
  }
  return ctx
}
