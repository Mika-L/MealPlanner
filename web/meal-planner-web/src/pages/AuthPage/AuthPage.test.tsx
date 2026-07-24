import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router'

import type { AuthResult } from '../../api/authTypes'
import * as authClient from '../../api/authClient'
import { AuthProvider } from '../../auth'
import { clearSession } from '../../auth/tokenStore'
import { AuthPage } from './AuthPage'

vi.mock('../../api/authClient', () => ({
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

function renderPage() {
  return render(
    <AuthProvider>
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/login" element={<AuthPage />} />
          <Route path="/" element={<p>Accueil</p>} />
        </Routes>
      </MemoryRouter>
    </AuthProvider>,
  )
}

describe('AuthPage', () => {
  beforeEach(() => {
    localStorage.clear()
    clearSession()
    vi.mocked(authClient.login).mockReset()
    vi.mocked(authClient.register).mockReset()
  })

  it('logs the user in and redirects to the home page', async () => {
    const user = userEvent.setup()
    vi.mocked(authClient.login).mockResolvedValue(authResult)
    renderPage()

    await user.type(screen.getByLabelText('Email'), 'alice@example.com')
    await user.type(screen.getByLabelText('Mot de passe'), 'Password1!')
    await user.click(screen.getByRole('button', { name: 'Se connecter' }))

    await waitFor(() => expect(screen.getByText('Accueil')).toBeInTheDocument())
    expect(authClient.login).toHaveBeenCalledWith({
      email: 'alice@example.com',
      password: 'Password1!',
    })
  })

  it('reveals the display name field when switching to registration', async () => {
    const user = userEvent.setup()
    renderPage()

    expect(screen.queryByLabelText('Nom affiché (optionnel)')).not.toBeInTheDocument()

    await user.click(screen.getByRole('tab', { name: 'Inscription' }))

    expect(screen.getByLabelText('Nom affiché (optionnel)')).toBeInTheDocument()
  })

  it('toggles the password between hidden and visible', async () => {
    const user = userEvent.setup()
    renderPage()

    const passwordField = screen.getByLabelText('Mot de passe')
    expect(passwordField).toHaveAttribute('type', 'password')

    await user.click(screen.getByRole('button', { name: 'Afficher le mot de passe' }))
    expect(passwordField).toHaveAttribute('type', 'text')

    await user.click(screen.getByRole('button', { name: 'Masquer le mot de passe' }))
    expect(passwordField).toHaveAttribute('type', 'password')
  })

  it('shows the password rules upfront and marks them as met while typing', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.click(screen.getByRole('tab', { name: 'Inscription' }))

    const rule = screen.getByText('Au moins 8 caractères')
    expect(rule).toBeInTheDocument()
    expect(rule).not.toHaveClass('auth__rule--met')

    await user.type(screen.getByLabelText('Mot de passe'), 'phrase de passe')

    expect(rule).toHaveClass('auth__rule--met')
  })

  it('shows the error returned by the API on a failed login', async () => {
    const user = userEvent.setup()
    vi.mocked(authClient.login).mockRejectedValue(new Error('Email ou mot de passe incorrect.'))
    renderPage()

    await user.type(screen.getByLabelText('Email'), 'alice@example.com')
    await user.type(screen.getByLabelText('Mot de passe'), 'wrong')
    await user.click(screen.getByRole('button', { name: 'Se connecter' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Email ou mot de passe incorrect.')
    expect(screen.queryByText('Accueil')).not.toBeInTheDocument()
  })
})
