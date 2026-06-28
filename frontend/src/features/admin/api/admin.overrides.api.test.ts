import { describe, expect, it } from 'vitest'
import {
  buildUserOverridesInvalidatesTags,
  buildUserOverridesProvidesTags,
  buildRevokeUserOverrideRequest,
  buildUpsertUserOverrideRequest,
  getGroupOverridesPath,
  getUserOverridesPath,
} from './admin.api'

describe('admin overrides endpoints (A4)', () => {
  it('defines getUserOverrides endpoint at /api/v1/admin/authorization/users/{userId}/overrides', () => {
    expect(getUserOverridesPath('u1')).toBe('/api/v1/admin/authorization/users/u1/overrides')
  })

  it('defines upsertUserOverride payload route and body', () => {
    expect(
      buildUpsertUserOverrideRequest('u1', {
        permissionId: 'p1',
        scopeType: 'Global',
        scopeValue: null,
        effect: 'Deny',
        reason: 'block',
        expiresAtUtc: null,
      }),
    ).toEqual({
      url: '/api/v1/admin/authorization/users/u1/overrides',
      method: 'POST',
      body: {
        permissionId: 'p1',
        scopeType: 'Global',
        scopeValue: null,
        effect: 'Deny',
        reason: 'block',
        expiresAtUtc: null,
      },
    })
  })

  it('defines revokeUserOverride route with required query params', () => {
    expect(
      buildRevokeUserOverrideRequest('u1', {
        permissionId: 'p1',
        scopeType: 'Global',
        scopeValue: null,
      }),
    ).toEqual({
      url: '/api/v1/admin/authorization/users/u1/overrides',
      method: 'DELETE',
      params: {
        permissionId: 'p1',
        scopeType: 'Global',
        scopeValue: '',
      },
    })
  })

  it('defines group override route helper for group tab support', () => {
    expect(getGroupOverridesPath('g1')).toBe('/api/v1/admin/authorization/groups/g1/overrides')
  })

  it('defines providesTags contract for user override list and entities', () => {
    expect(
      buildUserOverridesProvidesTags('u1', [
        {
          overrideId: 'o1',
          permissionId: 'p1',
          permissionCode: 'forum.post.create',
          scopeType: 'Global',
          scopeValue: null,
          effect: 'Deny',
          reason: null,
          expiresAtUtc: null,
          createdAtUtc: '2026-01-01T00:00:00Z',
          updatedAtUtc: null,
          isRevoked: false,
        },
      ]),
    ).toEqual([
      { type: 'UserOverride', id: 'u1:p1:Global:' },
      { type: 'UserOverride', id: 'LIST:u1' },
    ])
    expect(buildUserOverridesProvidesTags('u1', [])).toEqual([{ type: 'UserOverride', id: 'LIST:u1' }])
  })

  it('defines invalidatesTags contract to refresh override list after mutations', () => {
    expect(buildUserOverridesInvalidatesTags('u1')).toEqual([{ type: 'UserOverride', id: 'LIST:u1' }])
  })
})
