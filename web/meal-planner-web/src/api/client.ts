import { getAccessToken, getSession, refreshTokens } from '../auth/tokenStore'

/**
 * fetch authentifié : injecte le header Authorization à partir de la session courante.
 * Sur un 401, tente un refresh unique puis rejoue la requête ; si le refresh échoue,
 * la session est purgée (tokenStore) et le AuthProvider bascule en anonyme.
 */
export async function apiFetch(
  input: string,
  init: RequestInit = {},
  retryOnUnauthorized = true,
): Promise<Response> {
  const token = getAccessToken()
  const headers = new Headers(init.headers)
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(input, { ...init, headers })

  if (response.status === 401 && retryOnUnauthorized && getSession()?.refreshToken) {
    const refreshed = await refreshTokens()
    if (refreshed) {
      return apiFetch(input, init, false)
    }
  }

  return response
}
