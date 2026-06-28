import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import * as React from 'react'
import type { PermissionDto, RoleDto } from '../../types/admin.types'
import { AdminRolesPage } from '../components/AdminRolesPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

const roles: RoleDto[] = [
  {
    id: 'role-admin',
    name: 'Admin',
    description: 'Admin role',
    isDefault: false,
    isSystemRole: true,
    permissionCount: 0,
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: 'role-mod',
    name: 'Moderator',
    description: 'Moderator role',
    isDefault: false,
    isSystemRole: false,
    permissionCount: 0,
    createdAt: '2026-01-02T00:00:00Z',
  },
]

const permissions: PermissionDto[] = [
  {
    id: 'perm-delete-post',
    code: 'forum.post.delete',
    name: 'Delete post',
    description: 'Delete forum posts',
    module: 'forum',
    resource: 'post',
    action: 'delete',
  },
]

vi.mock('../hooks/useAdminRolesPage', () => {
  return {
    useAdminRolesPage: () => {
      const [selectedRoleId, setSelectedRoleId] = React.useState('role-admin')
      const [assigned, setAssigned] = React.useState<Record<string, string[]>>({
        'role-admin': [],
        'role-mod': [],
      })

      const selectedRole = roles.find((role) => role.id === selectedRoleId) ?? null

      return {
        t: (key: string) => key,
        roles,
        permissions,
        selectedRoleId,
        selectedRole,
        isLoading: false,
        isError: false,
        isCreateModalOpen: false,
        isCreatingRole: false,
        isAssigningPermission: false,
        isRemovingPermission: false,
        openCreateModal: vi.fn(),
        closeCreateModal: vi.fn(),
        selectRole: setSelectedRoleId,
        createRole: vi.fn(),
        isPermissionAssigned: (roleId: string, permissionId: string) =>
          (assigned[roleId] ?? []).includes(permissionId),
        togglePermission: (permissionId: string) => {
          setAssigned((prev) => {
            const current = prev[selectedRoleId] ?? []
            const next = current.includes(permissionId)
              ? current.filter((item) => item !== permissionId)
              : [...current, permissionId]
            return {
              ...prev,
              [selectedRoleId]: next,
            }
          })
        },
      }
    },
  }
})

describe('AdminRolesPage interactions', () => {
  it('selects role and toggles permission assignment', () => {
    render(<AdminRolesPage />)

    const moderatorButton = screen.getByRole('button', { name: /Moderator/ })
    fireEvent.click(moderatorButton)

    const permissionToggle = screen.getByRole('checkbox', { name: 'Delete post' })
    expect(permissionToggle).not.toBeChecked()

    fireEvent.click(permissionToggle)
    expect(permissionToggle).toBeChecked()
  })
})
