import { describe, expect, it } from 'vitest'
import {
  buildAssignBadgeRequest,
  buildAssignRoleToUserRequest,
  buildRemoveBadgeRequest,
  buildRemoveRoleFromUserRequest,
  getAssignUserRolePath,
  getRemoveUserRolePath,
  getUserBadgePath,
  getUserPath,
  getUsersPath,
} from './admin.api'

describe('admin users endpoints (A3)', () => {
  it('defines getUsers endpoint at /api/v1/users', () => {
    expect(getUsersPath()).toBe('/api/v1/users')
  })

  it('defines getUser endpoint at /api/v1/users/{id}', () => {
    expect(getUserPath('u1')).toBe('/api/v1/users/u1')
  })

  it('defines assignRoleToUser mutation payload contract', () => {
    expect(getAssignUserRolePath('u1')).toBe('/api/v1/users/u1/roles')
    expect(buildAssignRoleToUserRequest('u1', { roleId: 'r1' })).toEqual({
      url: '/api/v1/users/u1/roles',
      method: 'POST',
      body: { roleId: 'r1' },
    })
  })

  it('defines removeRoleFromUser mutation endpoint', () => {
    expect(getRemoveUserRolePath('u1', 'r1')).toBe('/api/v1/users/u1/roles/r1')
    expect(buildRemoveRoleFromUserRequest('u1', 'r1')).toEqual({
      url: '/api/v1/users/u1/roles/r1',
      method: 'DELETE',
    })
  })

  it('defines assignBadge mutation payload contract', () => {
    expect(getUserBadgePath('u1')).toBe('/api/v1/users/u1/badge')
    expect(
      buildAssignBadgeRequest('u1', {
        badgeType: 'Faculty',
        badgeName: 'Computer Science',
        description: 'Verified by admin',
      }),
    ).toEqual({
      url: '/api/v1/users/u1/badge',
      method: 'POST',
      body: {
        badgeType: 'Faculty',
        badgeName: 'Computer Science',
        description: 'Verified by admin',
      },
    })
  })

  it('defines removeBadge mutation endpoint', () => {
    expect(getUserBadgePath('u1')).toBe('/api/v1/users/u1/badge')
    expect(buildRemoveBadgeRequest('u1')).toEqual({
      url: '/api/v1/users/u1/badge',
      method: 'DELETE',
    })
  })
})
