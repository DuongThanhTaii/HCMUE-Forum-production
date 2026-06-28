import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { AdminUsersPage } from '../components/AdminUsersPage'

const mockHookResult = {
  t: (key: string) => key,
  users: [
    {
      id: 'u-1',
      fullName: 'Alice Nguyen',
      email: 'alice@hcmue.edu.vn',
      status: 'Active',
      roleIds: ['r-student'],
    },
  ],
  roleOptions: [
    { value: 'all', label: 'All roles' },
    { value: 'r-admin', label: 'Admin' },
    { value: 'r-student', label: 'Student' },
  ],
  statusOptions: [
    { value: 'all', label: 'All statuses' },
    { value: 'Active', label: 'Active' },
  ],
  roleListItems: [
    { id: 'r-admin', label: 'Admin' },
    { id: 'r-student', label: 'Student' },
  ],
  searchValue: '',
  roleFilter: 'all',
  statusFilter: 'all',
  editingRolesUserId: null,
  roleActionError: null,
  isRoleMutating: false,
  isLoading: false,
  isError: false,
  setSearchValue: vi.fn(),
  setRoleFilter: vi.fn(),
  setStatusFilter: vi.fn(),
  openRoleEditor: vi.fn(),
  closeRoleEditor: vi.fn(),
  assignRole: vi.fn(),
  removeRole: vi.fn(),
  getRoleLabels: () => ['Student'],
}

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

vi.mock('../hooks/useAdminUsersPage', () => {
  return {
    useAdminUsersPage: () => mockHookResult,
  }
})

describe('AdminUsersPage', () => {
  it('renders users table with roles column', () => {
    render(<AdminUsersPage />)

    expect(screen.getByText('Alice Nguyen')).toBeInTheDocument()
    expect(screen.getAllByText('Student').length).toBeGreaterThan(0)
    expect(screen.getByLabelText('admin.usersPage.filters.role')).toBeEnabled()

    fireEvent.click(screen.getByRole('button', { name: 'admin.usersPage.actions.manageRoles' }))
    expect(mockHookResult.openRoleEditor).toHaveBeenCalledWith('u-1')
  })
})
