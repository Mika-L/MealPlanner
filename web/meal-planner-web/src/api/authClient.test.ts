import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { AuthResult } from './authTypes'
import { login, register } from './authClient'

const authResult: AuthResult = {
  accessToken: 'access-1',
  refreshToken: 'refresh-1',
  expiresAt: '2099-01-01T00:00:00Z',
  user: { id: 'u1', email: 'alice@example.com', displayName: 'Alice' },
}

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

describe('authClient', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('logs in and returns the issued session', async () => {
    const fetchMock = vi.mocked(fetch)
    fetchMock.mockResolvedValue(jsonResponse(200, authResult))

    const result = await login({ email: 'alice@example.com', password: 'Password1!' })

    expect(result).toEqual(authResult)
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/auth/login',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ email: 'alice@example.com', password: 'Password1!' }),
      }),
    )
  })

  it('surfaces the ProblemDetails message when login fails', async () => {
    vi.mocked(fetch).mockResolvedValue(
      jsonResponse(401, { detail: 'Email ou mot de passe incorrect.' }),
    )

    await expect(login({ email: 'alice@example.com', password: 'wrong' })).rejects.toThrow(
      'Email ou mot de passe incorrect.',
    )
  })

  it('surfaces the first validation error when registration is invalid', async () => {
    vi.mocked(fetch).mockResolvedValue(
      jsonResponse(400, { errors: { Password: ['Le mot de passe est trop court.'] } }),
    )

    await expect(
      register({ email: 'alice@example.com', password: 'short' }),
    ).rejects.toThrow('Le mot de passe est trop court.')
  })

  it('sends a null display name when none is provided at registration', async () => {
    const fetchMock = vi.mocked(fetch)
    fetchMock.mockResolvedValue(jsonResponse(200, authResult))

    await register({ email: 'alice@example.com', password: 'Password1!' })

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/auth/register',
      expect.objectContaining({
        body: JSON.stringify({
          email: 'alice@example.com',
          password: 'Password1!',
          displayName: null,
        }),
      }),
    )
  })
})
