# TASK-114: Testing Setup & Implementation

> **Jest, React Testing Library, unit tests, integration tests, E2E**

---

## 📋 TASK INFO

| Property         | Value                            |
| ---------------- | -------------------------------- |
| **Task ID**      | TASK-114                         |
| **Module**       | Testing                          |
| **Status**       | ⬜ NOT_STARTED                   |
| **Priority**     | 🟡 Medium                        |
| **Estimate**     | 6 hours                          |
| **Branch**       | `feature/TASK-114-testing-setup` |
| **Dependencies** | TASK-101                          |

---

## 🎯 OBJECTIVES

- Configure Jest + React Testing Library
- Setup coverage thresholds
- Write unit tests for components
- Write integration tests for forms
- Setup E2E testing (optional)
- Add test utilities
- Configure CI/CD testing pipeline

---

## 📁 KEY FILES

### 1. Jest Configuration

**File**: `jest.config.js`

```javascript
const nextJest = require('next/jest');

const createJestConfig = nextJest({
  // Provide the path to your Next.js app to load next.config.js and .env files in your test environment
  dir: './',
});

const customJestConfig = {
  setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
  testEnvironment: 'jest-environment-jsdom',
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  collectCoverageFrom: [
    'src/**/*.{js,jsx,ts,tsx}',
    '!src/**/*.d.ts',
    '!src/**/*.stories.{js,jsx,ts,tsx}',
    '!src/**/__tests__/**',
    '!src/app/**/layout.tsx',
    '!src/app/**/page.tsx',
  ],
  coverageThresholds: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70,
    },
  },
  testPathIgnorePatterns: ['<rootDir>/.next/', '<rootDir>/node_modules/'],
  transformIgnorePatterns: [
    'node_modules/(?!(uuid|@microsoft/signalr)/)',
  ],
};

module.exports = createJestConfig(customJestConfig);
```

### 2. Jest Setup File

**File**: `jest.setup.js`

```javascript
import '@testing-library/jest-dom';
import { TextEncoder, TextDecoder } from 'util';

// Polyfills
global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder;

// Mock IntersectionObserver
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  unobserve() {}
  takeRecords() {
    return [];
  }
};

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Mock next/navigation
jest.mock('next/navigation', () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    prefetch: jest.fn(),
    back: jest.fn(),
  }),
  usePathname: () => '/',
  useSearchParams: () => new URLSearchParams(),
}));

// Mock next-intl
jest.mock('next-intl', () => ({
  useTranslations: () => (key) => key,
  useLocale: () => 'vi',
}));
```

### 3. Test Utilities

**File**: `src/test-utils/index.tsx`

```tsx
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { NextIntlClientProvider } from 'next-intl';

const messages = {
  // Add minimal translation messages for tests
};

function AllTheProviders({ children }: { children: React.ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <NextIntlClientProvider locale="vi" messages={messages}>
        {children}
      </NextIntlClientProvider>
    </QueryClientProvider>
  );
}

function customRender(
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, { wrapper: AllTheProviders, ...options });
}

export * from '@testing-library/react';
export { customRender as render };
```

### 4. Example: Component Unit Test

**File**: `src/components/ui/button.test.tsx`

```tsx
import { render, screen } from '@/test-utils';
import { Button } from './button';

describe('Button', () => {
  it('renders button with text', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole('button', { name: 'Click me' })).toBeInTheDocument();
  });

  it('applies variant styles', () => {
    render(<Button variant="destructive">Delete</Button>);
    const button = screen.getByRole('button', { name: 'Delete' });
    expect(button).toHaveClass('bg-destructive');
  });

  it('handles click events', () => {
    const handleClick = jest.fn();
    render(<Button onClick={handleClick}>Click</Button>);
    
    screen.getByRole('button').click();
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('is disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('renders as child component when asChild is true', () => {
    render(
      <Button asChild>
        <a href="/test">Link Button</a>
      </Button>
    );
    expect(screen.getByRole('link')).toHaveAttribute('href', '/test');
  });
});
```

### 5. Example: Form Integration Test

**File**: `src/components/features/auth/LoginForm.test.tsx`

```tsx
import { render, screen, waitFor } from '@/test-utils';
import { LoginForm } from './LoginForm';
import userEvent from '@testing-library/user-event';

// Mock the API
jest.mock('@/hooks/auth/useLogin', () => ({
  useLogin: () => ({
    mutate: jest.fn(),
    isPending: false,
  }),
}));

describe('LoginForm', () => {
  it('renders login form fields', () => {
    render(<LoginForm />);
    
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /đăng nhập/i })).toBeInTheDocument();
  });

  it('shows validation errors for empty fields', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    const submitButton = screen.getByRole('button', { name: /đăng nhập/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email không được để trống/i)).toBeInTheDocument();
      expect(screen.getByText(/mật khẩu không được để trống/i)).toBeInTheDocument();
    });
  });

  it('shows validation error for invalid email', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);

    const emailInput = screen.getByLabelText(/email/i);
    await user.type(emailInput, 'invalid-email');
    await user.tab(); // Trigger blur

    await waitFor(() => {
      expect(screen.getByText(/email không hợp lệ/i)).toBeInTheDocument();
    });
  });

  it('submits form with valid data', async () => {
    const user = userEvent.setup();
    const mockLogin = jest.fn();
    
    jest.spyOn(require('@/hooks/auth/useLogin'), 'useLogin').mockReturnValue({
      mutate: mockLogin,
      isPending: false,
    });

    render(<LoginForm />);

    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'Password123!');
    await user.click(screen.getByRole('button', { name: /đăng nhập/i }));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'Password123!',
      });
    });
  });
});
```

### 6. Example: API Hook Test

**File**: `src/hooks/api/auth/useLogin.test.ts`

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useLogin } from './useLogin';
import * as authApi from '@/lib/api/auth.api';

jest.mock('@/lib/api/auth.api');

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useLogin', () => {
  it('successful login returns token', async () => {
    const mockResponse = {
      accessToken: 'mock-token',
      refreshToken: 'mock-refresh',
      user: { id: '1', email: 'test@example.com' },
    };

    jest.spyOn(authApi, 'login').mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useLogin(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      email: 'test@example.com',
      password: 'password123',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(mockResponse);
  });

  it('failed login returns error', async () => {
    const mockError = new Error('Invalid credentials');
    jest.spyOn(authApi, 'login').mockRejectedValue(mockError);

    const { result } = renderHook(() => useLogin(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      email: 'test@example.com',
      password: 'wrong-password',
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toEqual(mockError);
  });
});
```

### 7. Example: Store Test (Zustand)

**File**: `src/stores/auth.store.test.ts`

```typescript
import { renderHook, act } from '@testing-library/react';
import { useAuthStore } from './auth.store';

describe('AuthStore', () => {
  beforeEach(() => {
    // Reset store before each test
    useAuthStore.setState({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
    });
  });

  it('initial state is unauthenticated', () => {
    const { result } = renderHook(() => useAuthStore());
    
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.token).toBeNull();
  });

  it('setAuth updates authentication state', () => {
    const { result } = renderHook(() => useAuthStore());
    
    const mockUser = { id: '1', email: 'test@example.com' };
    const mockToken = 'mock-token';
    const mockRefreshToken = 'mock-refresh';

    act(() => {
      result.current.setAuth({
        user: mockUser,
        token: mockToken,
        refreshToken: mockRefreshToken,
      });
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual(mockUser);
    expect(result.current.token).toBe(mockToken);
    expect(result.current.refreshToken).toBe(mockRefreshToken);
  });

  it('logout clears authentication state', () => {
    const { result } = renderHook(() => useAuthStore());
    
    // First login
    act(() => {
      result.current.setAuth({
        user: { id: '1', email: 'test@example.com' },
        token: 'mock-token',
        refreshToken: 'mock-refresh',
      });
    });

    // Then logout
    act(() => {
      result.current.logout();
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.token).toBeNull();
    expect(result.current.refreshToken).toBeNull();
  });
});
```

### 8. Package.json Test Scripts

**File**: `package.json` (add scripts)

```json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage",
    "test:ci": "jest --ci --coverage --maxWorkers=2",
    "test:debug": "node --inspect-brk node_modules/.bin/jest --runInBand"
  }
}
```

### 9. GitHub Actions CI Workflow

**File**: `.github/workflows/test.yml`

```yaml
name: Testing

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run linter
        run: npm run lint
      
      - name: Run tests
        run: npm run test:ci
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/lcov.info
```

---

## ✅ ACCEPTANCE CRITERIA

- [ ] Jest configuration complete
- [ ] React Testing Library setup
- [ ] Test utilities with providers
- [ ] Unit tests for UI components (Button, Input, etc.)
- [ ] Unit tests for feature components
- [ ] Integration tests for forms
- [ ] API hooks tests
- [ ] Store tests (Zustand)
- [ ] Coverage threshold 70%+
- [ ] CI/CD pipeline with tests
- [ ] Test scripts in package.json
- [ ] Mock setup for Next.js routing
- [ ] Mock setup for i18n
- [ ] Documentation for writing tests

---

## 🧪 TESTING BEST PRACTICES

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **Test user behavior**: Focus on what users see and do
3. **Avoid implementation details**: Don't test internal state
4. **Use semantic queries**: `getByRole`, `getByLabelText` over `getByTestId`
5. **Mock external dependencies**: API calls, SignalR, localStorage
6. **Keep tests isolated**: Reset state between tests
7. **Write descriptive test names**: `it('shows error when email is invalid')`

---

_Last Updated: 2026-02-10_
