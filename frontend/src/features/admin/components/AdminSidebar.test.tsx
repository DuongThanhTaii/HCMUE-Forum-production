import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AdminSidebar } from './AdminSidebar';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

describe('AdminSidebar', () => {
  const activeCases = [
    ['/admin/users', 'admin.users'],
    ['/admin/roles', 'admin.roles'],
    ['/admin/overrides/users', 'admin.overrides.users'],
    ['/admin/overrides/groups', 'admin.overrides.groups'],
    ['/admin/toggles', 'admin.toggles'],
    ['/admin/forum/thread-channels', 'admin.threadChannels'],
    ['/admin/logs/actions', 'admin.logs.actions'],
    ['/admin/logs/audit', 'admin.logs.audit'],
  ] as const;

  it.each(activeCases)('marks %s as active navigation item', (pathname, expectedLabel) => {
    render(
      <MemoryRouter initialEntries={[pathname]}>
        <AdminSidebar />
      </MemoryRouter>,
    );

    const activeLink = screen.getByRole('link', { name: expectedLabel });
    expect(activeLink).toHaveAttribute('aria-current', 'page');
  });

  it('renders a back button to /home', () => {
    render(
      <MemoryRouter initialEntries={['/admin/users']}>
        <AdminSidebar />
      </MemoryRouter>,
    );

    const backButton = screen.getByRole('link', { name: 'common.back' });
    expect(backButton).toHaveAttribute('href', '/home');
  });
});
