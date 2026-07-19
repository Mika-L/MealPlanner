import type { AuthResult } from '../api/authTypes'

// Source de vérité des jetons, hors React : le wrapper apiFetch et le AuthProvider
// s'appuient tous deux dessus. La session est persistée pour survivre à un reload.

const storageKey = 'mealplanner-auth'

let session: AuthResult | null = load()
const listeners = new Set<() => void>()

function load(): AuthResult | null {
  try {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return null
    }
    const parsed = JSON.parse(raw) as AuthResult
    return parsed.accessToken && parsed.refreshToken ? parsed : null
  } catch {
    return null
  }
}

function persist(): void {
  try {
    if (session) {
      localStorage.setItem(storageKey, JSON.stringify(session))
    } else {
      localStorage.removeItem(storageKey)
    }
  } catch {
    // Persistance best-effort : on ignore un localStorage indisponible.
  }
}

function notify(): void {
  listeners.forEach((listener) => listener())
}

export function getSession(): AuthResult | null {
  return session
}

export function getAccessToken(): string | null {
  return session?.accessToken ?? null
}

export function setSession(next: AuthResult): void {
  session = next
  persist()
  notify()
}

export function clearSession(): void {
  if (session === null) {
    return
  }
  session = null
  persist()
  notify()
}

/** S'abonne aux changements de session (connexion, déconnexion, refresh). */
export function subscribe(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

let refreshPromise: Promise<boolean> | null = null

/**
 * Échange le refresh token contre un nouveau couple de jetons.
 * Single-flight : plusieurs appels concurrents partagent la même requête.
 * Purge la session en cas d'échec. Renvoie true si la session a été rafraîchie.
 */
export function refreshTokens(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise
  }

  const current = session
  if (!current?.refreshToken) {
    return Promise.resolve(false)
  }

  refreshPromise = fetch('/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: current.refreshToken }),
  })
    .then(async (response) => {
      if (!response.ok) {
        clearSession()
        return false
      }
      const next = (await response.json()) as AuthResult
      setSession(next)
      return true
    })
    .catch(() => {
      clearSession()
      return false
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}
