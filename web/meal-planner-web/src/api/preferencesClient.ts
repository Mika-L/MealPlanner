import { apiFetch } from './client'

export interface Preferences {
  theme: string
}

export async function getPreferences(signal?: AbortSignal): Promise<Preferences> {
  const response = await apiFetch('/api/preferences', { signal })

  if (!response.ok) {
    throw new Error(`Le chargement des préférences a échoué (HTTP ${response.status}).`)
  }

  return (await response.json()) as Preferences
}

export async function updatePreferences(theme: string, signal?: AbortSignal): Promise<void> {
  const response = await apiFetch('/api/preferences', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ theme }),
    signal,
  })

  if (!response.ok) {
    throw new Error(`L'enregistrement des préférences a échoué (HTTP ${response.status}).`)
  }
}
