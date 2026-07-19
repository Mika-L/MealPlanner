import type { AuthResult, AuthenticatedUser, LoginInput, RegisterInput } from './authTypes'
import { apiFetch } from './client'

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

/** Extrait un message lisible d'une réponse d'erreur ProblemDetails de l'API. */
async function readError(response: Response, fallback: string): Promise<string> {
  try {
    const problem = (await response.json()) as ProblemDetails
    const firstValidation = problem.errors && Object.values(problem.errors)[0]?.[0]
    return problem.detail ?? firstValidation ?? problem.title ?? fallback
  } catch {
    return fallback
  }
}

async function postAuth(path: string, body: unknown, fallback: string): Promise<AuthResult> {
  const response = await fetch(path, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw new Error(await readError(response, fallback))
  }

  return (await response.json()) as AuthResult
}

export function register(input: RegisterInput): Promise<AuthResult> {
  return postAuth(
    '/api/auth/register',
    { email: input.email, password: input.password, displayName: input.displayName || null },
    "La création du compte a échoué.",
  )
}

export function login(input: LoginInput): Promise<AuthResult> {
  return postAuth('/api/auth/login', input, 'Email ou mot de passe incorrect.')
}

export function loginWithGoogle(idToken: string): Promise<AuthResult> {
  return postAuth('/api/auth/google', { idToken }, 'La connexion Google a échoué.')
}

export function loginWithFacebook(accessToken: string): Promise<AuthResult> {
  return postAuth('/api/auth/facebook', { accessToken }, 'La connexion Facebook a échoué.')
}

export async function getCurrentUser(signal?: AbortSignal): Promise<AuthenticatedUser> {
  const response = await apiFetch('/api/auth/me', { signal })

  if (!response.ok) {
    throw new Error(await readError(response, 'La récupération du profil a échoué.'))
  }

  return (await response.json()) as AuthenticatedUser
}
