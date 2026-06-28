import { act, renderHook } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { useAdminLogsPage } from '../hooks/useAdminLogsPage'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

const mockUseGetTogglesQuery = vi.fn()
const mockUseSetToggleMutation = vi.fn()
const mockUseGetAuditLogsQuery = vi.fn()
const mockUseGetUserActionLogsQuery = vi.fn()

vi.mock('../../api/admin.observability.api', () => ({
  useGetTogglesQuery: () => mockUseGetTogglesQuery(),
  useSetToggleMutation: () => mockUseSetToggleMutation(),
  useGetMaintenanceModeQuery: () => ({ data: null, isLoading: false, isError: false }),
  useSetMaintenanceModeMutation: () => [vi.fn(), { isLoading: false }],
  useGetAuditLogsQuery: (params: unknown) => mockUseGetAuditLogsQuery(params),
  useGetUserActionLogsQuery: (params: unknown) => mockUseGetUserActionLogsQuery(params),
}))

describe('useAdminLogsPage', () => {
  it('uses default action page size 100', () => {
    mockUseGetTogglesQuery.mockReturnValue({ data: [], isLoading: false, isError: false })
    mockUseSetToggleMutation.mockReturnValue([vi.fn(), { isLoading: false }])
    mockUseGetAuditLogsQuery.mockReturnValue({ data: [], isLoading: false, isError: false })
    mockUseGetUserActionLogsQuery.mockReturnValue({
      data: {
        items: [],
        total: 0,
        page: 1,
        pageSize: 100,
        viewType: 'Developer',
        availableViewTypes: ['Developer', 'Administrator'],
        persistToMongo: true,
        mongoCollectionName: 'user_action_logs',
      },
      isLoading: false,
      isError: false,
    })

    const { result } = renderHook(() => useAdminLogsPage())
    expect(result.current.actionPageSize).toBe(100)
  })

  it('switches view type and resets action logs page', () => {
    mockUseGetTogglesQuery.mockReturnValue({ data: [], isLoading: false, isError: false })
    mockUseSetToggleMutation.mockReturnValue([vi.fn(), { isLoading: false }])
    mockUseGetAuditLogsQuery.mockReturnValue({ data: [], isLoading: false, isError: false })
    mockUseGetUserActionLogsQuery.mockImplementation((params: unknown) => {
      const parsed = (params ?? {}) as { page?: number; pageSize?: number; viewType?: 'Developer' | 'Administrator' }
      return {
      data: {
        items: [],
        total: 0,
        page: parsed.page ?? 1,
        pageSize: parsed.pageSize ?? 100,
        viewType: parsed.viewType ?? 'Developer',
        availableViewTypes: ['Developer', 'Administrator'],
        persistToMongo: true,
        mongoCollectionName: 'user_action_logs',
      },
      isLoading: false,
      isError: false,
    }})

    const { result } = renderHook(() => useAdminLogsPage())

    act(() => {
      result.current.setActionPage(3)
    })
    expect(result.current.actionPage).toBe(3)

    act(() => {
      result.current.setActionViewType('Administrator')
    })

    expect(result.current.actionViewType).toBe('Administrator')
    expect(result.current.actionPage).toBe(1)
  })
})
