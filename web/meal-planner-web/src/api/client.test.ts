import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { clearSession, getSession, setSession } from '../auth/tokenStore'
import type { AuthResult } from './authTypes'
import { apiFetch } from './client'

function session(overrides: Partial<AuthResult> = {}): AuthResult {
  return {
    accessToken: 'access-1',
    refreshToken: 'refresh-1',
    expiresAt: '2099-01-01T00:00:00Z',
    user: { id: 'u1', email: 'alice@example.com', displayName: null },
    ...overrides,
  }
}

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

function authHeaderOf(call: [unknown, RequestInit?] | undefined): string | null {
  return new Headers(call?.[1]?.headers).get('Authorization')
}

describe('apiFetch', () => {
  beforeEach(() => {
    localStorage.clear()
    clearSession()
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('joins the access token as a Bearer Authorization header', async () => {
    setSession(session())
    const fetchMock = vi.mocked(fetch)
    fetchMock.mockResolvedValue(jsonResponse(200, { ok: true }))

    await apiFetch('/api/meals')

    expect(authHeaderOf(fetchMock.mock.calls[0])).toBe('Bearer access-1')
  })

  it('sends no Authorization header when anonymous', async () => {
    const fetchMock = vi.mocked(fetch)
    fetchMock.mockResolvedValue(jsonResponse(200, { ok: true }))

    await apiFetch('/api/meals')

    expect(authHeaderOf(fetchMock.mock.calls[0])).toBeNull()
  })

  it('refreshes the session and replays the request after a 401', async () => {
    setSession(session())
    const fetchMock = vi.mocked(fetch)
    fetchMock
      .mockResolvedValueOnce(jsonResponse(401, {}))
      .mockResolvedValueOnce(jsonResponse(200, session({ accessToken: 'access-2' })))
      .mockResolvedValueOnce(jsonResponse(200, { ideas: [] }))

    const response = await apiFetch('/api/meals')

    expect(response.status).toBe(200)
    expect(fetchMock).toHaveBeenCalledTimes(3)
    expect(fetchMock.mock.calls[1][0]).toBe('/api/auth/refresh')
    expect(authHeaderOf(fetchMock.mock.calls[2])).toBe('Bearer access-2')
    expect(getSession()?.accessToken).toBe('access-2')
  })

  it('clears the session when the refresh itself fails', async () => {
    setSession(session())
    const fetchMock = vi.mocked(fetch)
    fetchMock
      .mockResolvedValueOnce(jsonResponse(401, {}))
      .mockResolvedValueOnce(jsonResponse(401, {}))

    const response = await apiFetch('/api/meals')

    expect(response.status).toBe(401)
    expect(getSession()).toBeNull()
  })

  it('does not attempt a refresh when there is no session', async () => {
    const fetchMock = vi.mocked(fetch)
    fetchMock.mockResolvedValue(jsonResponse(401, {}))

    const response = await apiFetch('/api/meals')

    expect(response.status).toBe(401)
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })
})
