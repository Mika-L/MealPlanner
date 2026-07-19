import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router'

import { useAuth } from '../../auth'

type Mode = 'login' | 'register'

interface LocationState {
  from?: { pathname: string }
}

export function AuthPage() {
  const { status, login, register } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [mode, setMode] = useState<Mode>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
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

  return (
    <div className="auth">
      <section className="auth__card" aria-labelledby="auth-title">
        <h1 id="auth-title" className="auth__title">
          {mode === 'login' ? 'Connexion' : 'Créer un compte'}
        </h1>

        <div className="auth__tabs" role="tablist" aria-label="Connexion ou inscription">
          <button
            type="button"
            role="tab"
            aria-selected={mode === 'login'}
            className={`auth__tab${mode === 'login' ? ' auth__tab--active' : ''}`}
            onClick={() => switchMode('login')}
          >
            Connexion
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={mode === 'register'}
            className={`auth__tab${mode === 'register' ? ' auth__tab--active' : ''}`}
            onClick={() => switchMode('register')}
          >
            Inscription
          </button>
        </div>

        <form className="auth__form" onSubmit={(event) => void handleSubmit(event)}>
          {mode === 'register' && (
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

          <label>
            Mot de passe
            <input
              type="password"
              value={password}
              required
              minLength={mode === 'register' ? 8 : undefined}
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>

          {error && (
            <p className="auth__error" role="alert">
              {error}
            </p>
          )}

          <button type="submit" className="auth__submit" disabled={submitting}>
            {submitting
              ? 'Un instant…'
              : mode === 'login'
                ? 'Se connecter'
                : "S'inscrire"}
          </button>
        </form>

        <div className="auth__divider" aria-hidden="true">
          ou
        </div>

        <div className="auth__social">
          <button type="button" className="auth__social-btn" disabled>
            Continuer avec Google
          </button>
          <button type="button" className="auth__social-btn" disabled>
            Continuer avec Facebook
          </button>
          <p className="auth__social-hint">Connexion Google et Facebook bientôt disponible.</p>
        </div>
      </section>
    </div>
  )
}
