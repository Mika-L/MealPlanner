import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { AuthResult } from '../api/authTypes'
import * as authClient from '../api/authClient'
import { AuthProvider } from './AuthProvider'
import { useAuth } from './AuthContext'
import { clearSession, setSession } from './tokenStore'

vi.mock('../api/authClient', () => ({
  login: vi.fn(),
  register: vi.fn(),
  loginWithGoogle: vi.fn(),
  getCurrentUser: vi.fn(),
}))

const authResult: AuthResult = {
  accessToken: 'access-1',
  refreshToken: 'refresh-1',
  expiresAt: '2099-01-01T00:00:00Z',
  user: { id: 'u1', email: 'alice@example.com', displayName: 'Alice' },
}

function Harness() {
  const { status, user, login, logout } = useAuth()
  return (
    <div>
      <span data-testid="status">{status}</span>
      <span data-testid="user">{user?.email ?? '—'}</span>
      <button onClick={() => void login({ email: 'alice@example.com', password: 'Password1!' })}>
        login
      </button>
      <button onClick={logout}>logout</button>
    </div>
  )
}

function renderHarness() {
  return render(
    <AuthProvider>
      <Harness />
    </AuthProvider>,
  )
}

describe('AuthProvider', () => {
  beforeEach(() => {
    localStorage.clear()
    clearSession()
    vi.mocked(authClient.login).mockReset()
    vi.mocked(authClient.getCurrentUser).mockReset()
  })

  it('starts anonymous with no persisted session', () => {
    renderHarness()

    expect(screen.getByTestId('status')).toHaveTextContent('anonymous')
    expect(authClient.getCurrentUser).not.toHaveBeenCalled()
  })

  it('authenticates the user after a successful login', async () => {
    const user = userEvent.setup()
    vi.mocked(authClient.login).mockResolvedValue(authResult)
    renderHarness()

    await user.click(screen.getByRole('button', { name: 'login' }))

    await waitFor(() => expect(screen.getByTestId('status')).toHaveTextContent('authenticated'))
    expect(screen.getByTestId('user')).toHaveTextContent('alice@example.com')
  })

  it('returns to anonymous after logout', async () => {
    const user = userEvent.setup()
    vi.mocked(authClient.login).mockResolvedValue(authResult)
    renderHarness()

    await user.click(screen.getByRole('button', { name: 'login' }))
    await waitFor(() => expect(screen.getByTestId('status')).toHaveTextContent('authenticated'))

    await user.click(screen.getByRole('button', { name: 'logout' }))

    expect(screen.getByTestId('status')).toHaveTextContent('anonymous')
    expect(screen.getByTestId('user')).toHaveTextContent('—')
  })

  it('restores a persisted session and revalidates it on mount', async () => {
    setSession(authResult)
    vi.mocked(authClient.getCurrentUser).mockResolvedValue({
      id: 'u1',
      email: 'alice.new@example.com',
      displayName: 'Alice',
    })

    renderHarness()

    expect(screen.getByTestId('status')).toHaveTextContent('authenticated')
    expect(authClient.getCurrentUser).toHaveBeenCalled()
    await waitFor(() => expect(screen.getByTestId('user')).toHaveTextContent('alice.new@example.com'))
  })
})
