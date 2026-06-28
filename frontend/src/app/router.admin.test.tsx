import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import type { RouteObject } from 'react-router-dom'

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}))

vi.mock('@shared/components/layouts/AuthLayout', async () => {
  const { Outlet } = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { AuthLayout: () => <Outlet /> }
})

vi.mock('@shared/components/layouts/MainLayout', async () => {
  const { Outlet } = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { MainLayout: () => <Outlet /> }
})

vi.mock('@shared/components/layouts/ModLayout', async () => {
  const { Outlet } = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { ModLayout: () => <Outlet /> }
})

vi.mock('@shared/components/layouts/AdminLayout', async () => {
  const { Outlet } = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return { AdminLayout: () => <Outlet /> }
})

vi.mock('@features/forum/components/HomePage', () => ({
  HomePage: () => <div>Home page</div>,
}))
vi.mock('@features/forum/components/ForumListPage', () => ({
  ForumListPage: () => <div>Forum page</div>,
}))
vi.mock('@features/forum/components/ForumDetailPage', () => ({
  ForumDetailPage: () => <div>Forum detail page</div>,
}))
vi.mock('@features/learning/components/LearningDocumentsPage', () => ({
  LearningDocumentsPage: () => <div>Learning documents page</div>,
}))
vi.mock('@features/career/components/CareerJobsPage', () => ({
  CareerJobsPage: () => <div>Career jobs page</div>,
}))
vi.mock('@features/auth/components/LoginPage', () => ({
  LoginPage: () => <div>Login page</div>,
}))
vi.mock('@features/auth/components/RegisterPage', () => ({
  RegisterPage: () => <div>Register page</div>,
}))
vi.mock('@features/admin/users/components/AdminUsersPage', () => ({
  AdminUsersPage: () => <div>Admin users page</div>,
}))

const mockedUseAppSelector = vi.fn()
const mockedUseAuth = vi.fn()

vi.mock('@features/auth/context/useAuth', () => ({
  useAuth: () => mockedUseAuth(),
}))

vi.mock('@shared/hooks/useAppSelector', () => ({
  useAppSelector: (selector: unknown) => mockedUseAppSelector(selector),
}))

const buildRouter = async (initialPath: string) => {
  vi.resetModules()
  const { appRoutes } = await import('./router')
  return createMemoryRouter(appRoutes as RouteObject[], { initialEntries: [initialPath] })
}

describe('admin routes in app router', () => {
  beforeEach(() => {
    vi.stubEnv('VITE_DEV_BYPASS_AUTH', 'false')
    mockedUseAuth.mockReset()
    mockedUseAppSelector.mockReset()
  })

  afterEach(() => {
    vi.unstubAllEnvs()
  })

  it('redirects non-admin from /admin/users via actual router tree', async () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: true })
    mockedUseAppSelector.mockReturnValue(['Student'])

    const router = await buildRouter('/admin/users')

    render(<RouterProvider router={router} />)

    await screen.findByText('Home page', {}, { timeout: 10000 })
    expect(router.state.location.pathname).toBe('/home')
  })

  it('allows authenticated admin to access /admin/users', async () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: true })
    mockedUseAppSelector.mockReturnValue(['Admin'])

    const router = await buildRouter('/admin/users')

    render(<RouterProvider router={router} />)

    await screen.findAllByText('Admin users page')
    expect(router.state.location.pathname).toBe('/admin/users')
  })

  it('redirects unauthenticated users to /login from /admin/users', async () => {
    mockedUseAuth.mockReturnValue({ isAuthenticated: false })
    mockedUseAppSelector.mockReturnValue([])

    const router = await buildRouter('/admin/users')

    render(<RouterProvider router={router} />)

    await screen.findByText('Login page')
    expect(router.state.location.pathname).toBe('/login')
  })
})
