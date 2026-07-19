import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router'

import type { AuthResult } from '../../api/authTypes'
import { AuthProvider } from '../../auth'
import { clearSession, setSession } from '../../auth/tokenStore'
import { RequireAuth } from './RequireAuth'

const authResult: AuthResult = {
  accessToken: 'access-1',
  refreshToken: 'refresh-1',
  expiresAt: '2099-01-01T00:00:00Z',
  user: { id: 'u1', email: 'alice@example.com', displayName: null },
}

function renderAt(path: string) {
  return render(
    <AuthProvider>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/login" element={<p>Page de connexion</p>} />
          <Route
            path="/secret"
            element={
              <RequireAuth>
                <p>Contenu protégé</p>
              </RequireAuth>
            }
          />
        </Routes>
      </MemoryRouter>
    </AuthProvider>,
  )
}

describe('RequireAuth', () => {
  beforeEach(() => {
    localStorage.clear()
    clearSession()
  })

  it('redirects an anonymous visitor to the login page', () => {
    renderAt('/secret')

    expect(screen.getByText('Page de connexion')).toBeInTheDocument()
    expect(screen.queryByText('Contenu protégé')).not.toBeInTheDocument()
  })

  it('renders the protected content for an authenticated user', () => {
    setSession(authResult)
    renderAt('/secret')

    expect(screen.getByText('Contenu protégé')).toBeInTheDocument()
  })
})
