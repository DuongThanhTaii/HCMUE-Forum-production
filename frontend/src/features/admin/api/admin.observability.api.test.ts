import { describe, expect, it } from 'vitest'
import {
  buildAuditLogsParams,
  buildSetToggleRequest,
  buildUserActionLogsParams,
  getAuditLogsPath,
  getTogglePath,
  getTogglesPath,
  getUserActionLogsPath,
} from './admin.observability.api'

describe('admin observability endpoints (A5)', () => {
  it('defines toggles routes', () => {
    expect(getTogglesPath()).toBe('/api/v1/admin/authorization/toggles')
    expect(getTogglePath('Api.Identity.AuthorizationAdmin.SetEndpointToggle')).toBe(
      '/api/v1/admin/authorization/toggles/Api.Identity.AuthorizationAdmin.SetEndpointToggle',
    )
    expect(getTogglePath('Api Identity/AuthorizationAdmin.SetEndpointToggle')).toBe(
      '/api/v1/admin/authorization/toggles/Api%20Identity%2FAuthorizationAdmin.SetEndpointToggle',
    )
  })

  it('defines setToggle payload route and body', () => {
    expect(
      buildSetToggleRequest('Api.Identity.AuthorizationAdmin.SetEndpointToggle', {
        isEnabled: false,
        reason: 'Maintenance window',
      }),
    ).toEqual({
      url: '/api/v1/admin/authorization/toggles/Api.Identity.AuthorizationAdmin.SetEndpointToggle',
      method: 'PUT',
      body: {
        isEnabled: false,
        reason: 'Maintenance window',
      },
    })
  })

  it('defines audit logs route and compacts undefined params', () => {
    expect(getAuditLogsPath()).toBe('/api/v1/admin/authorization/audit-logs')
    expect(
      buildAuditLogsParams({
        userId: 'u1',
        endpointKey: undefined,
        isSuccess: true,
        fromUtc: '2026-04-29T00:00:00Z',
        toUtc: undefined,
        take: 100,
      }),
    ).toEqual({
      userId: 'u1',
      isSuccess: true,
      fromUtc: '2026-04-29T00:00:00Z',
      take: 100,
    })
  })

  it('defines user action logs route and supports documented defaults', () => {
    expect(getUserActionLogsPath()).toBe('/api/v1/admin/observability/user-actions')
    expect(
      buildUserActionLogsParams({
        actorUserId: 'u1',
        viewType: 'Developer',
        page: 1,
        pageSize: 100,
      }),
    ).toEqual({
      actorUserId: 'u1',
      viewType: 'Developer',
      page: 1,
      pageSize: 100,
    })
  })
})
