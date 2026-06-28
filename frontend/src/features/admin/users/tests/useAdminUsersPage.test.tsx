import { act, renderHook } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { useAdminUsersPage } from '../hooks/useAdminUsersPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

const mockUseGetUsersQuery = vi.fn()
const mockUseGetRolesQuery = vi.fn()
const mockAssignRoleMutation = vi.fn()
const mockRemoveRoleMutation = vi.fn()

vi.mock('../../api/admin.api', () => ({
  useGetUsersQuery: () => mockUseGetUsersQuery(),
  useGetRolesQuery: () => mockUseGetRolesQuery(),
  useAssignRoleToUserMutation: () => mockAssignRoleMutation(),
  useRemoveRoleFromUserMutation: () => mockRemoveRoleMutation(),
}))

describe('useAdminUsersPage', () => {
  it('filters users by role when roleIds are present', () => {
    mockUseGetUsersQuery.mockReturnValue({
      data: [
        {
          id: 'u-1',
          email: 'alice@hcmue.edu.vn',
          fullName: 'Alice Nguyen',
          bio: null,
          status: 'Active',
          badge: null,
          roleIds: ['r-student'],
          createdAt: '2026-01-01T00:00:00Z',
        },
        {
          id: 'u-2',
          email: 'admin@hcmue.edu.vn',
          fullName: 'Admin User',
          bio: null,
          status: 'Active',
          badge: null,
          roleIds: ['r-admin'],
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      isLoading: false,
      isError: false,
    })
    mockUseGetRolesQuery.mockReturnValue({
      data: [
        {
          id: 'r-admin',
          name: 'Admin',
          description: '',
          isDefault: false,
          isSystemRole: true,
          permissionCount: 0,
          createdAt: '2026-01-01T00:00:00Z',
        },
        {
          id: 'r-student',
          name: 'Student',
          description: '',
          isDefault: true,
          isSystemRole: true,
          permissionCount: 0,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      isLoading: false,
      isError: false,
    })
    mockAssignRoleMutation.mockReturnValue([vi.fn(), { isLoading: false }])
    mockRemoveRoleMutation.mockReturnValue([vi.fn(), { isLoading: false }])

    const { result } = renderHook(() => useAdminUsersPage())
    expect(result.current.users).toHaveLength(2)

    act(() => {
      result.current.setRoleFilter('r-admin')
    })

    expect(result.current.roleFilter).toBe('r-admin')
    expect(result.current.users).toHaveLength(1)
    expect(result.current.users[0]?.id).toBe('u-2')
  })
})
