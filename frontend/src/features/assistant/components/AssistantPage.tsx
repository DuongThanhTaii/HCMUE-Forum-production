import { useEffect, useMemo, useRef, useState } from 'react'
import { useExchangeUEBotTokenMutation } from '../api/assistant.api'
import { useAppSelector } from '@shared/hooks/useAppSelector'

const UEBOT_WEBAPP_URL = import.meta.env.VITE_UEBOT_WEBAPP_URL || 'http://localhost:3000'
const UEBOT_SYNC_API_URL = import.meta.env.VITE_UEBOT_SYNC_API_URL || ''
const MSG_SYNC_AUTH = 'UNIHUB_SYNC_AUTH'
const MSG_SYNC_INIT = 'UNIHUB_SYNC_INIT'
const MSG_SYNC_REFRESH = 'UNIHUB_SYNC_REFRESH'
const MSG_UEBOT_READY = 'UEBOT_SYNC_READY'
const MSG_UEBOT_REQUEST_REFRESH = 'UEBOT_SYNC_REQUEST_REFRESH'

function extractIssuer(accessToken: string | null | undefined): string {
  if (!accessToken) return ''
  try {
    const parts = accessToken.split('.')
    if (parts.length < 2) return ''
    const normalized = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
    const payload = JSON.parse(atob(padded)) as { iss?: string }
    return typeof payload.iss === 'string' ? payload.iss : ''
  } catch {
    return ''
  }
}

export function AssistantPage() {
  const iframeRef = useRef<HTMLIFrameElement | null>(null)
  const accessToken = useAppSelector((state) => state.auth.accessToken)
  const [exchangeToken, { isLoading, isError, error }] = useExchangeUEBotTokenMutation()
  const [exchangeResult, setExchangeResult] = useState<{
    syncAccessToken: string
    syncApiBaseUrl: string
    syncExpiresAt?: string | null
  } | null>(null)
  const [handshakeNonce] = useState(() => {
    if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
      return crypto.randomUUID()
    }
    return `${Date.now()}-${Math.random().toString(16).slice(2)}`
  })
  const tokenIssuer = useMemo(() => extractIssuer(accessToken), [accessToken])
  const shouldUseDirectAzureTrust = useMemo(() => {
    if (!accessToken || !UEBOT_SYNC_API_URL) return false
    return (
      tokenIssuer.includes('login.microsoftonline.com') ||
      tokenIssuer.includes('sts.windows.net')
    )
  }, [accessToken, tokenIssuer])

  const targetOrigin = useMemo(() => {
    try {
      return new URL(UEBOT_WEBAPP_URL).origin
    } catch {
      return ''
    }
  }, [])

  const postSyncPayload = (
    type: string,
    payload: {
      token: string
      syncApiBaseUrl: string
      expiresAt: string | null
      forumAccessToken?: string | null
      forumApiUrl?: string | null
    }
  ) => {
    if (!iframeRef.current?.contentWindow || !targetOrigin) return
    iframeRef.current.contentWindow.postMessage(
      {
        type,
        payload: {
          ...payload,
          nonce: handshakeNonce,
          forumOrigin: window.location.origin,
          forumApiUrl: window.location.origin,
        },
      },
      targetOrigin
    )
  }

  const refreshExchangeToken = async () => {
    if (!accessToken) return null

    if (shouldUseDirectAzureTrust) {
      const directResult = {
        syncAccessToken: accessToken,
        syncApiBaseUrl: UEBOT_SYNC_API_URL,
        syncExpiresAt: null,
      }
      setExchangeResult(directResult)
      return directResult
    }

    try {
      const data = await exchangeToken().unwrap()
      setExchangeResult(data)
      return data
    } catch {
      setExchangeResult(null)
      return null
    }
  }

  useEffect(() => {
    if (!accessToken) {
      setExchangeResult(null)
      return
    }

    void refreshExchangeToken()
  }, [accessToken, exchangeToken, shouldUseDirectAzureTrust])

  useEffect(() => {
    if (!exchangeResult) return
    postSyncPayload(MSG_SYNC_AUTH, {
      token: exchangeResult.syncAccessToken,
      syncApiBaseUrl: exchangeResult.syncApiBaseUrl,
      expiresAt: exchangeResult.syncExpiresAt ?? null,
      forumAccessToken: accessToken ?? null,
    })
  }, [exchangeResult, targetOrigin, handshakeNonce, accessToken])

  useEffect(() => {
    if (!targetOrigin) return
    const handler = (event: MessageEvent) => {
      if (event.origin !== targetOrigin) return
      if (event.source !== iframeRef.current?.contentWindow) return
      const data = event.data as { type?: string; payload?: { nonce?: string } } | null
      if (!data?.type) return
      if (data.payload?.nonce && data.payload.nonce !== handshakeNonce) return

      if (data.type === MSG_UEBOT_READY && exchangeResult) {
        postSyncPayload(MSG_SYNC_INIT, {
          token: exchangeResult.syncAccessToken,
          syncApiBaseUrl: exchangeResult.syncApiBaseUrl,
          expiresAt: exchangeResult.syncExpiresAt ?? null,
          forumAccessToken: accessToken ?? null,
        })
      }

      if (data.type === MSG_UEBOT_REQUEST_REFRESH) {
        if (shouldUseDirectAzureTrust && accessToken) {
          postSyncPayload(MSG_SYNC_REFRESH, {
            token: accessToken,
            syncApiBaseUrl: UEBOT_SYNC_API_URL,
            expiresAt: null,
            forumAccessToken: accessToken,
          })
          return
        }
        void refreshExchangeToken().then((next) => {
          if (!next) return
          postSyncPayload(MSG_SYNC_REFRESH, {
            token: next.syncAccessToken,
            syncApiBaseUrl: next.syncApiBaseUrl,
            expiresAt: next.syncExpiresAt ?? null,
            forumAccessToken: accessToken ?? null,
          })
        })
      }
    }

    window.addEventListener('message', handler)
    return () => {
      window.removeEventListener('message', handler)
    }
  }, [targetOrigin, exchangeResult, handshakeNonce, shouldUseDirectAzureTrust, accessToken])

  useEffect(() => {
    if (shouldUseDirectAzureTrust) return
    if (!exchangeResult) return
    const timer = window.setInterval(() => {
      void refreshExchangeToken().then((next) => {
        if (!next) return
        postSyncPayload(MSG_SYNC_REFRESH, {
          token: next.syncAccessToken,
          syncApiBaseUrl: next.syncApiBaseUrl,
          expiresAt: next.syncExpiresAt ?? null,
          forumAccessToken: accessToken ?? null,
        })
      })
    }, 8 * 60 * 1000)

    return () => {
      window.clearInterval(timer)
    }
  }, [exchangeResult, handshakeNonce, targetOrigin, shouldUseDirectAzureTrust, accessToken])

  const iframeSrc = useMemo(() => {
    if (!exchangeResult) return UEBOT_WEBAPP_URL
    const url = new URL(UEBOT_WEBAPP_URL)
    url.searchParams.set('authToken', exchangeResult.syncAccessToken)
    url.searchParams.set('syncApiUrl', exchangeResult.syncApiBaseUrl)
    url.searchParams.set('forumOrigin', window.location.origin)
    url.searchParams.set('forumApiUrl', window.location.origin)
    url.searchParams.set('handshakeNonce', handshakeNonce)
    return url.toString()
  }, [exchangeResult, handshakeNonce])

  const handleRetry = async () => {
    try {
      const data = await exchangeToken().unwrap()
      setExchangeResult(data)
    } catch {
      setExchangeResult(null)
    }
  }

  if (isLoading && !exchangeResult && !shouldUseDirectAzureTrust) {
    return (
      <section className="mx-auto w-full max-w-7xl px-4 py-6">
        <div className="rounded-lg border bg-card p-6 text-sm text-muted-foreground">
          Establishing secure assistant session...
        </div>
      </section>
    )
  }

  if ((isError || (!exchangeResult && !shouldUseDirectAzureTrust)) && !isLoading) {
    return (
      <section className="mx-auto w-full max-w-7xl px-4 py-6">
        <div className="space-y-4 rounded-lg border bg-card p-6">
          <p className="text-sm text-destructive">
            Unable to start the assistant session.
            {' '}
            {typeof error === 'object' && error && 'status' in error
              ? `(${String(error.status)})`
              : ''}
          </p>
          <button
            type="button"
            onClick={handleRetry}
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
          >
            Retry
          </button>
        </div>
      </section>
    )
  }

  return (
    <section className="mx-auto w-full max-w-7xl px-4 py-4">
      <div className="overflow-hidden rounded-lg border bg-card">
        <iframe
          ref={iframeRef}
          title="UEBot Assistant"
          src={iframeSrc}
          className="h-[calc(100vh-170px)] w-full border-0"
          allow="clipboard-read; clipboard-write"
          onLoad={() => {
            if (exchangeResult && iframeRef.current?.contentWindow) {
              postSyncPayload(MSG_SYNC_INIT, {
                token: exchangeResult.syncAccessToken,
                syncApiBaseUrl: exchangeResult.syncApiBaseUrl,
                expiresAt: exchangeResult.syncExpiresAt ?? null,
              })
            }
          }}
        />
      </div>
    </section>
  )
}
