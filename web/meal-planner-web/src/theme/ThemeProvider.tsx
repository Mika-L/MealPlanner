import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import type { PropsWithChildren } from 'react'

import { getPreferences, updatePreferences } from '../api/preferencesClient'
import { useAuth } from '../auth'
import { ThemeContext, type ThemeContextValue } from './ThemeContext'
import { defaultTheme, isThemeId, themeStorageKey, type ThemeId } from './themes'

function readStoredTheme(): ThemeId {
  try {
    const stored = localStorage.getItem(themeStorageKey)
    return isThemeId(stored) ? stored : defaultTheme
  } catch {
    return defaultTheme
  }
}

export function ThemeProvider({ children }: PropsWithChildren) {
  const { status } = useAuth()
  const [theme, setThemeState] = useState<ThemeId>(readStoredTheme)

  // Applique la palette au document et la mémorise localement (repli hors ligne / anonyme).
  useEffect(() => {
    document.documentElement.dataset.palette = theme
    try {
      localStorage.setItem(themeStorageKey, theme)
    } catch {
      // Persistance best-effort : on ignore un localStorage indisponible.
    }
  }, [theme])

  // À la connexion, le thème serveur fait autorité (setThemeState direct → pas de PUT en écho).
  useEffect(() => {
    if (status !== 'authenticated') {
      return
    }
    const controller = new AbortController()
    getPreferences(controller.signal)
      .then((prefs) => {
        if (isThemeId(prefs.theme)) {
          setThemeState(prefs.theme)
        }
      })
      .catch(() => {
        // Chargement best-effort : on garde la palette locale si l'API échoue.
      })
    return () => controller.abort()
  }, [status])

  // Référence de statut lue par setTheme pour rester stable (pas de dépendance au status).
  const authenticatedRef = useRef(status === 'authenticated')
  authenticatedRef.current = status === 'authenticated'

  // setTheme = choix explicite de l'utilisateur → persiste côté serveur si connecté.
  const setTheme = useCallback((next: ThemeId) => {
    setThemeState(next)
    if (authenticatedRef.current) {
      updatePreferences(next).catch(() => {
        // Enregistrement best-effort : la palette reste appliquée localement.
      })
    }
  }, [])

  const value = useMemo<ThemeContextValue>(() => ({ theme, setTheme }), [theme, setTheme])

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}
