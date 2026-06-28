import { baseApi } from '@shared/lib/api/baseApi'
import type { AuthPayload } from '@shared/types/auth'
import { logout, setAuth } from '../model/auth.slice'
import { parseRolesFromAccessToken } from '../lib/token'

interface LoginRequest {
  email: string
  password: string
}

interface RegisterRequest {
  email: string
  password: string
  fullName: string
  bio?: string
}

type ApiEnvelope<T> = {
  success?: boolean
  message?: string
  data?: T
}

type LoginResponseData = {
  accessToken?: string
  refreshToken?: string
}

type RegisterResponseData = {
  userId?: string
  email?: string
  fullName?: string
}

type UserMeResponseData = {
  id?: string
  email?: string
  fullName?: string
  bio?: string | null
}

const toAuthPayload = (tokens: LoginResponseData, me: UserMeResponseData): AuthPayload => {
  const accessToken = tokens.accessToken?.trim()
  const refreshToken = tokens.refreshToken?.trim()

  if (!accessToken || !refreshToken) {
    throw new Error('Invalid login token payload')
  }

  const id = me.id?.trim()
  const email = me.email?.trim()
  const fullName = me.fullName?.trim()

  if (!id || !email || !fullName) {
    throw new Error('Invalid user profile payload')
  }

  return {
    accessToken,
    refreshToken,
    user: {
      id,
      email,
      fullName,
      roles: parseRolesFromAccessToken(accessToken),
    },
  }
}

export const authApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<AuthPayload, LoginRequest>({
      async queryFn(body, _api, _extraOptions, fetchWithBQ) {
        const loginResult = await fetchWithBQ({
          url: '/api/v1/auth/login',
          method: 'POST',
          body,
        })

        if (loginResult.error) {
          return { error: loginResult.error }
        }

        try {
          const tokens = (loginResult.data as ApiEnvelope<LoginResponseData>)?.data
          if (!tokens?.accessToken || !tokens.refreshToken) {
            return {
              error: {
                status: 'CUSTOM_ERROR',
                error: 'Invalid login token payload',
                data: loginResult.data,
              },
            }
          }

          const meResult = await fetchWithBQ({
            url: '/api/v1/users/me',
            method: 'GET',
            headers: {
              authorization: `Bearer ${tokens.accessToken}`,
            },
          })

          if (meResult.error) {
            return { error: meResult.error }
          }

          const me = (meResult.data as ApiEnvelope<UserMeResponseData>)?.data
          return { data: toAuthPayload(tokens, me ?? {}) }
        } catch (error) {
          return {
            error: {
              status: 'CUSTOM_ERROR',
              error: error instanceof Error ? error.message : 'Login transform failed',
              data: null,
            },
          }
        }
      },
      async onQueryStarted(_arg, { dispatch, queryFulfilled }) {
        try {
          const { data } = await queryFulfilled
          dispatch(setAuth(data))
        } catch {
          dispatch(logout())
        }
      },
    }),
    register: builder.mutation<RegisterResponseData, RegisterRequest>({
      query: (body) => ({
        url: '/api/v1/auth/register',
        method: 'POST',
        body,
      }),
      transformResponse: (response: ApiEnvelope<RegisterResponseData>) => {
        const data = response?.data
        if (!data?.userId || !data?.email) {
          throw new Error('Invalid register payload')
        }
        return data
      },
    }),
  }),
})

export const { useLoginMutation, useRegisterMutation } = authApi

