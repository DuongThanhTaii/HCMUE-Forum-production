import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ReplyPreviewBar } from './ReplyPreviewBar'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, opts?: { name?: string }) =>
      key === 'chat.reply.replyingTo' ? `Reply to ${opts?.name}` : key,
  }),
}))

describe('ReplyPreviewBar', () => {
  it('shows preview and sender', () => {
    render(
      <ReplyPreviewBar
        target={{
          messageId: 'm1',
          preview: 'Hello world',
          senderLabel: 'Alice',
        }}
        onClear={() => {}}
      />,
    )
    expect(screen.getByText('Reply to Alice')).toBeTruthy()
    expect(screen.getByText('Hello world')).toBeTruthy()
  })
})
