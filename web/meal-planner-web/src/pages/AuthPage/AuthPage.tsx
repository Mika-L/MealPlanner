import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router'

import { GoogleSignInButton } from '../../components/GoogleSignInButton/GoogleSignInButton'
import { useAuth } from '../../auth'

type Mode = 'login' | 'register'

interface LocationState {
  from?: { pathname: string }
}

// Politique alignée sur le back (NIST SP 800-63B) : la longueur prime, aucune règle de composition.
const MIN_PASSWORD_LENGTH = 8

export function AuthPage() {
  const { status, login, register, loginWithGoogle } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [mode, setMode] = useState<Mode>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const redirectTo = (location.state as LocationState | null)?.from?.pathname ?? '/'

  // Un utilisateur déjà authentifié n'a rien à faire ici (retour arrière, session restaurée).
  useEffect(() => {
    if (status === 'authenticated') {
      navigate(redirectTo, { replace: true })
    }
  }, [status, navigate, redirectTo])

  const switchMode = (next: Mode) => {
    setMode(next)
    setError(null)
  }

  const handleGoogleCredential = async (idToken: string) => {
    setError(null)
    setSubmitting(true)
    try {
      await loginWithGoogle(idToken)
      navigate(redirectTo, { replace: true })
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'La connexion Google a échoué.')
    } finally {
      setSubmitting(false)
    }
  }

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      if (mode === 'login') {
        await login({ email: email.trim(), password })
      } else {
        await register({ email: email.trim(), password, displayName: displayName.trim() })
      }
      navigate(redirectTo, { replace: true })
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Une erreur est survenue.')
    } finally {
      setSubmitting(false)
    }
  }

  const isRegister = mode === 'register'
  const meetsMinLength = password.length >= MIN_PASSWORD_LENGTH

  return (
    <div className="auth">
      <section className="auth__card" aria-labelledby="auth-title">
        <h1 id="auth-title" className="auth__title">
          {isRegister ? 'Créer un compte' : 'Connexion'}
        </h1>

        <div className="auth__tabs" role="tablist" aria-label="Connexion ou inscription">
          <button
            type="button"
            role="tab"
            aria-selected={!isRegister}
            className={`auth__tab${!isRegister ? ' auth__tab--active' : ''}`}
            onClick={() => switchMode('login')}
          >
            Connexion
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={isRegister}
            className={`auth__tab${isRegister ? ' auth__tab--active' : ''}`}
            onClick={() => switchMode('register')}
          >
            Inscription
          </button>
        </div>

        <form className="auth__form" onSubmit={(event) => void handleSubmit(event)}>
          {isRegister && (
            <label>
              Nom affiché (optionnel)
              <input
                type="text"
                value={displayName}
                maxLength={100}
                autoComplete="name"
                onChange={(event) => setDisplayName(event.target.value)}
              />
            </label>
          )}

          <label>
            Email
            <input
              type="email"
              value={email}
              required
              autoComplete="email"
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>

          <div className="auth__field">
            <label htmlFor="auth-password">Mot de passe</label>
            <div className="auth__password">
              {/* Aucun handler onPaste : le collage (gestionnaires de mots de passe) reste autorisé. */}
              <input
                id="auth-password"
                type={showPassword ? 'text' : 'password'}
                value={password}
                required
                minLength={isRegister ? MIN_PASSWORD_LENGTH : undefined}
                autoComplete={isRegister ? 'new-password' : 'current-password'}
                aria-describedby={isRegister ? 'auth-password-rules' : undefined}
                onChange={(event) => setPassword(event.target.value)}
              />
              <button
                type="button"
                className="auth__password-toggle"
                onClick={() => setShowPassword((visible) => !visible)}
                aria-pressed={showPassword}
                aria-label={showPassword ? 'Masquer le mot de passe' : 'Afficher le mot de passe'}
                title={showPassword ? 'Masquer le mot de passe' : 'Afficher le mot de passe'}
              >
                <EyeIcon crossed={showPassword} />
              </button>
            </div>

            {isRegister && (
              <ul id="auth-password-rules" className="auth__rules">
                <li className={`auth__rule${meetsMinLength ? ' auth__rule--met' : ''}`}>
                  <span className="auth__rule-mark" aria-hidden="true">
                    {meetsMinLength ? '✓' : '○'}
                  </span>
                  Au moins {MIN_PASSWORD_LENGTH} caractères
                  <span className="visually-hidden">
                    {meetsMinLength ? ' — critère rempli' : ' — critère non rempli'}
                  </span>
                </li>
                <li className="auth__hint">
                  Astuce : une phrase de passe longue (plusieurs mots) est plus sûre et plus facile à
                  retenir. Espaces et emojis sont acceptés.
                </li>
              </ul>
            )}
          </div>

          {error && (
            <p className="auth__error" role="alert">
              {error}
            </p>
          )}

          <button type="submit" className="auth__submit" disabled={submitting}>
            {submitting ? 'Un instant…' : isRegister ? "S'inscrire" : 'Se connecter'}
          </button>
        </form>

        <div className="auth__divider" aria-hidden="true">
          ou
        </div>

        <div className="auth__social">
          <GoogleSignInButton
            onCredential={(idToken) => void handleGoogleCredential(idToken)}
            disabled={submitting}
          />
          <button type="button" className="auth__social-btn" disabled>
            Continuer avec Facebook
          </button>
          <p className="auth__social-hint">Connexion Facebook bientôt disponible.</p>
        </div>
      </section>
    </div>
  )
}

/** Icône œil (ouvert / barré) pour le bouton afficher/masquer le mot de passe. */
function EyeIcon({ crossed }: { crossed: boolean }) {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z" />
      <circle cx="12" cy="12" r="3" />
      {crossed && <line x1="3" y1="3" x2="21" y2="21" />}
    </svg>
  )
}
