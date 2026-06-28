const ROLE_CLAIM_KEYS = [
  'role',
  'roles',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role',
]

type JwtPayload = Record<string, unknown>

function decodeBase64Url(value: string) {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/')
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
  return atob(padded)
}

export function parseRolesFromAccessToken(accessToken: string): string[] {
  try {
    const parts = accessToken.split('.')
    if (parts.length < 2) {
      return []
    }

    const payload = JSON.parse(decodeBase64Url(parts[1])) as JwtPayload
    const roles = ROLE_CLAIM_KEYS.flatMap((key) => {
      const value = payload[key]
      if (typeof value === 'string') {
        return value.trim() ? [value.trim()] : []
      }
      if (Array.isArray(value)) {
        return value
          .filter((item): item is string => typeof item === 'string')
          .map((item) => item.trim())
          .filter(Boolean)
      }
      return []
    })

    return Array.from(new Set(roles))
  } catch {
    return []
  }
}

type ParsedIdentity = {
  id: string | null
  email: string | null
  fullName: string | null
}

const ID_CLAIM_KEYS = ['user_id', 'oid', 'sub', 'nameid']
const EMAIL_CLAIM_KEYS = [
  'email',
  'preferred_username',
  'upn',
  'unique_name',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
]
const NAME_CLAIM_KEYS = [
  'profile_name',
  'name',
  'given_name',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
]

function decodePayload(accessToken: string): JwtPayload | null {
  try {
    const parts = accessToken.split('.')
    if (parts.length < 2) return null
    return JSON.parse(decodeBase64Url(parts[1])) as JwtPayload
  } catch {
    return null
  }
}

function pickString(payload: JwtPayload, keys: string[]): string | null {
  for (const key of keys) {
    const value = payload[key]
    if (typeof value === 'string' && value.trim()) {
      return value.trim()
    }
  }
  return null
}

export function parseIdentityFromAccessToken(accessToken: string): ParsedIdentity {
  const payload = decodePayload(accessToken)
  if (!payload) {
    return { id: null, email: null, fullName: null }
  }

  return {
    id: pickString(payload, ID_CLAIM_KEYS),
    email: pickString(payload, EMAIL_CLAIM_KEYS),
    fullName: pickString(payload, NAME_CLAIM_KEYS),
  }
}
