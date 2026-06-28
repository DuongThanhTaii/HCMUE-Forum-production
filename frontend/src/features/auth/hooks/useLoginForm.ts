import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { PublicClientApplication, type AccountInfo, type AuthenticationResult } from '@azure/msal-browser'
import { useLoginMutation } from '../api/auth.api'
import { useAppDispatch } from '@shared/hooks/useAppDispatch'
import { setAuth } from '../model/auth.slice'
import { parseIdentityFromAccessToken, parseRolesFromAccessToken } from '../lib/token'

const normalizeRoles = (roles: string[] | undefined) =>
  (roles ?? []).map((role) => role.trim().toLowerCase())

const hasRole = (roles: string[] | undefined, target: string) =>
  normalizeRoles(roles).includes(target)

const parseBooleanEnv = (value: unknown, defaultValue: boolean) => {
  if (typeof value !== 'string') return defaultValue
  const normalized = value.trim().toLowerCase()
  if (['1', 'true', 'yes', 'on'].includes(normalized)) return true
  if (['0', 'false', 'no', 'off'].includes(normalized)) return false
  return defaultValue
}

const toErrorMessage = (error: unknown) => {
  if (error && typeof error === 'object') {
    const errorCode =
      'errorCode' in error && typeof error.errorCode === 'string' ? error.errorCode : ''
    const message = 'message' in error && typeof error.message === 'string' ? error.message : ''
    if (errorCode && message) return `${errorCode}: ${message}`
    if (message) return message
  }
  return 'Azure sign-in failed. Please check app permissions and try again.'
}

export function useLoginForm() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const dispatch = useAppDispatch()
  const [login, { isLoading }] = useLoginMutation()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [isAzureLoading, setIsAzureLoading] = useState(false)
  const azureLoginInProgressRef = useRef(false)
  const tenantId = (import.meta.env.VITE_AZURE_TENANT_ID as string | undefined)?.trim() || ''
  const clientId = (import.meta.env.VITE_AZURE_CLIENT_ID as string | undefined)?.trim() || ''
  const apiScope = (import.meta.env.VITE_AZURE_API_SCOPE as string | undefined)?.trim() || ''
  const azureRedirectUri = `${window.location.origin}/login`
  const isAzureLoginFeatureEnabled = parseBooleanEnv(import.meta.env.VITE_ENABLE_AZURE_LOGIN, false)
  const isAzureLoginAvailable = isAzureLoginFeatureEnabled && Boolean(tenantId && clientId && apiScope)

  const createMsalClient = () =>
    new PublicClientApplication({
      auth: {
        clientId,
        authority: `https://login.microsoftonline.com/${tenantId}`,
        redirectUri: azureRedirectUri,
      },
      cache: {
        cacheLocation: 'localStorage',
      },
    })

  const completeAzureLogin = (accessToken: string, authResult?: AuthenticationResult) => {
    const identity = parseIdentityFromAccessToken(accessToken)
    const claims = (authResult?.idTokenClaims ?? {}) as Record<string, unknown>
    const account: AccountInfo | null | undefined = authResult?.account

    const claimEmail =
      (typeof claims.email === 'string' && claims.email.trim()) ||
      (typeof claims.preferred_username === 'string' && claims.preferred_username.trim()) ||
      (typeof claims.upn === 'string' && claims.upn.trim()) ||
      null

    const claimId =
      (typeof claims.oid === 'string' && claims.oid.trim()) ||
      (typeof claims.sub === 'string' && claims.sub.trim()) ||
      null

    const resolvedId = identity.id ?? claimId ?? account?.localAccountId ?? account?.homeAccountId ?? null
    const resolvedEmail = identity.email ?? claimEmail ?? account?.username ?? null
    const claimName = typeof claims.name === 'string' && claims.name.trim() ? claims.name.trim() : null
    const resolvedFullName = identity.fullName ?? claimName ?? account?.name ?? resolvedEmail

    console.log('[AzureAuth] resolved identity', {
      tokenId: identity.id,
      tokenEmail: identity.email,
      claimId,
      claimEmail,
      accountUsername: account?.username,
      resolvedId,
      resolvedEmail,
    })

    if (!resolvedId || !resolvedEmail) {
      throw new Error('Azure token is missing identity claims.')
    }

    const roles = parseRolesFromAccessToken(accessToken)
    const authPayload = {
      accessToken,
      refreshToken: '',
      user: {
        id: resolvedId,
        email: resolvedEmail,
        fullName: resolvedFullName || resolvedEmail,
        roles,
      },
    }

    dispatch(setAuth(authPayload))
    if (hasRole(roles, 'admin')) {
      navigate('/admin')
      return
    }
    if (hasRole(roles, 'moderator')) {
      navigate('/mod/reports')
      return
    }
    if (hasRole(roles, 'recruiter')) {
      navigate('/career/company-dashboard')
      return
    }
    navigate('/home')
  }

  useEffect(() => {
    if (!isAzureLoginAvailable) return
    let cancelled = false

    const handleRedirectResult = async () => {
      console.log('[AzureAuth] init redirect handler', {
        tenantId,
        clientId,
        apiScope,
        redirectUri: azureRedirectUri,
        href: window.location.href,
        hash: window.location.hash,
      })
      const msal = createMsalClient()
      await msal.initialize()

      const redirectResult = await msal.handleRedirectPromise()
      console.log('[AzureAuth] handleRedirectPromise result', redirectResult)
      if (!redirectResult) return

      let accessToken = redirectResult.accessToken?.trim()
      if (!accessToken && redirectResult.account) {
        console.log('[AzureAuth] no accessToken in redirectResult, trying acquireTokenSilent')
        const tokenResult = await msal.acquireTokenSilent({
          account: redirectResult.account,
          scopes: [apiScope],
        })
        accessToken = tokenResult.accessToken?.trim()
        console.log('[AzureAuth] acquireTokenSilent done', {
          hasAccessToken: Boolean(accessToken),
          account: tokenResult.account?.username,
        })
      }

      if (!accessToken) {
        throw new Error('Azure access token is empty.')
      }

      console.log('[AzureAuth] completeAzureLogin start')
      completeAzureLogin(accessToken, redirectResult)
    }

    void handleRedirectResult().catch((error) => {
      console.error('[AzureAuth] redirect handler error', error)
      if (!cancelled) {
        setErrorMessage(toErrorMessage(error))
      }
    })

    return () => {
      cancelled = true
    }
  }, [apiScope, isAzureLoginAvailable])

  const onAzureSignIn = async () => {
    setErrorMessage('')
    if (azureLoginInProgressRef.current) {
      setErrorMessage('Microsoft login is already in progress. Please finish the popup first.')
      return
    }

    if (!isAzureLoginAvailable) {
      setErrorMessage('Azure login is disabled or not configured.')
      return
    }

    azureLoginInProgressRef.current = true
    setIsAzureLoading(true)
    try {
      console.log('[AzureAuth] loginRedirect start', {
        tenantId,
        clientId,
        apiScope,
        redirectUri: azureRedirectUri,
      })
      const msal = createMsalClient()
      await msal.initialize()
      await msal.loginRedirect({
        scopes: ['openid', 'profile', apiScope],
        prompt: 'select_account',
      })
    } catch (error) {
      console.error('[AzureAuth] loginRedirect error', error)
      setErrorMessage(toErrorMessage(error))
      azureLoginInProgressRef.current = false
      setIsAzureLoading(false)
    } finally {
      // loginRedirect navigates away; keep loading state until redirect flow completes.
    }
  }

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setErrorMessage('')

    if (!email.trim() || !password.trim()) {
      setErrorMessage(t('auth.pages.validation.loginRequired'))
      return
    }

    try {
      const authPayload = await login({ email: email.trim(), password }).unwrap()
      // Ensure auth state is available before route guards evaluate navigation target.
      dispatch(setAuth(authPayload))
      const roles = authPayload.user.roles
      if (hasRole(roles, 'admin')) {
        navigate('/admin')
        return
      }
      if (hasRole(roles, 'moderator')) {
        navigate('/mod/reports')
        return
      }
      if (hasRole(roles, 'recruiter')) {
        navigate('/career/company-dashboard')
        return
      }
      navigate('/home')
    } catch {
      setErrorMessage(t('auth.invalidCredentials'))
    }
  }

  return {
    email,
    password,
    errorMessage,
    isLoading,
    isAzureLoading,
    isAzureLoginAvailable,
    setEmail,
    setPassword,
    onSubmit,
    onAzureSignIn,
  }
}
