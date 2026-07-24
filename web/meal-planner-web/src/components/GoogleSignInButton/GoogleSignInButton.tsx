import { useEffect, useRef } from 'react'

import { loadGoogleIdentityServices } from './loadGoogleIdentityServices'

// ID client OAuth Google (public). Absent → le bouton reste inactif (dégradation gracieuse) :
// la connexion par email/mot de passe demeure disponible.
const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID

interface GoogleSignInButtonProps {
  /** Reçoit l'id_token Google (JWT) une fois l'utilisateur authentifié côté Google. */
  onCredential: (idToken: string) => void
  /** Bloque l'interaction (ex. connexion en cours). */
  disabled?: boolean
}

export function GoogleSignInButton({ onCredential, disabled }: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  // Toujours appeler la dernière version du callback sans réinitialiser GIS.
  const onCredentialRef = useRef(onCredential)
  onCredentialRef.current = onCredential

  useEffect(() => {
    if (!GOOGLE_CLIENT_ID) {
      return
    }

    let cancelled = false
    loadGoogleIdentityServices()
      .then(() => {
        if (cancelled || !containerRef.current || !window.google) {
          return
        }
        window.google.accounts.id.initialize({
          client_id: GOOGLE_CLIENT_ID,
          callback: (response) => onCredentialRef.current(response.credential),
        })
        window.google.accounts.id.renderButton(containerRef.current, {
          type: 'standard',
          theme: 'outline',
          size: 'large',
          text: 'continue_with',
          shape: 'pill',
          logo_alignment: 'center',
          locale: 'fr',
        })
      })
      .catch(() => {
        // SDK Google indisponible (réseau, bloqueur de scripts) : bouton non rendu.
      })

    return () => {
      cancelled = true
    }
  }, [])

  // Sans ID client, on affiche un bouton inactif cohérent avec le reste de l'écran.
  if (!GOOGLE_CLIENT_ID) {
    return (
      <button type="button" className="auth__social-btn" disabled>
        Continuer avec Google
      </button>
    )
  }

  // Google impose son propre bouton (branding). On neutralise les clics pendant une connexion.
  return (
    <div
      ref={containerRef}
      className={`auth__google${disabled ? ' auth__google--busy' : ''}`}
    />
  )
}
