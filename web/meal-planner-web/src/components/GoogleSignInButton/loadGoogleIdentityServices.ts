// Charge le SDK Google Identity Services (bouton « Sign in with Google » + id_token).
// Idempotent : un seul <script> injecté, la promesse est partagée entre appels concurrents.

const SCRIPT_SRC = 'https://accounts.google.com/gsi/client'

let loadPromise: Promise<void> | null = null

export function loadGoogleIdentityServices(): Promise<void> {
  // Déjà disponible (SDK chargé, ou stubbé en test).
  if (window.google?.accounts?.id) {
    return Promise.resolve()
  }

  if (loadPromise) {
    return loadPromise
  }

  loadPromise = new Promise<void>((resolve, reject) => {
    const fail = () => {
      loadPromise = null
      reject(new Error('Échec du chargement de Google Identity Services.'))
    }

    const existing = document.querySelector<HTMLScriptElement>(`script[src="${SCRIPT_SRC}"]`)
    if (existing) {
      existing.addEventListener('load', () => resolve())
      existing.addEventListener('error', fail)
      return
    }

    const script = document.createElement('script')
    script.src = SCRIPT_SRC
    script.async = true
    script.defer = true
    script.onload = () => resolve()
    script.onerror = fail
    document.head.appendChild(script)
  })

  return loadPromise
}
