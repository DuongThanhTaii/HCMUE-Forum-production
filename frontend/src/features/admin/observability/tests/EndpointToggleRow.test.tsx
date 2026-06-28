import { act, fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { EndpointToggleRow } from '../components/EndpointToggleRow'
import type { EndpointToggleDto } from '../../types/admin.types'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) =>
      (
        {
          'admin.togglesPage.row.reasonRequired': 'Reason required',
        } as Record<string, string>
      )[key] ?? key,
  }),
}))

function makeToggle(overrides?: Partial<EndpointToggleDto>): EndpointToggleDto {
  return {
    endpointKey: 'Api.Identity.AuthorizationAdmin.SetEndpointToggle',
    isEnabled: true,
    reason: null,
    updatedBy: 'admin-1',
    updatedAtUtc: '2026-04-30T00:00:00Z',
    version: 1,
    ...overrides,
  }
}

describe('EndpointToggleRow', () => {
  it('requires reason when disabling endpoint', async () => {
    const onSubmit = vi.fn().mockResolvedValue(undefined)
    render(<EndpointToggleRow toggle={makeToggle()} isSubmitting={false} onSubmit={onSubmit} />)

    // Find the switch button
    const switchButton = screen.getByRole('switch')
    fireEvent.click(switchButton)

    await act(async () => {
      // Find confirm disable button (Xác nhận Tắt)
      fireEvent.click(screen.getByRole('button', { name: 'Xác nhận Tắt' }))
    })
    expect(onSubmit).not.toHaveBeenCalled()

    // Find the textarea by placeholder
    const textarea = screen.getByPlaceholderText('Nhập lý do tắt API...')
    fireEvent.change(textarea, { target: { value: 'Maintenance window' } })
    
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: 'Xác nhận Tắt' }))
    })
    expect(onSubmit).toHaveBeenCalledWith('Api.Identity.AuthorizationAdmin.SetEndpointToggle', false, 'Maintenance window')
  })
})
