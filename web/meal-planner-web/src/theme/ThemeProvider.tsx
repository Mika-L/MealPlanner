import { useCallback, useEffect, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'

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
  const [theme, setThemeState] = useState<ThemeId>(readStoredTheme)

  useEffect(() => {
    document.documentElement.dataset.palette = theme
    try {
      localStorage.setItem(themeStorageKey, theme)
    } catch {
      // Persistance best-effort : on ignore un localStorage indisponible.
    }
  }, [theme])

  const setTheme = useCallback((next: ThemeId) => {
    setThemeState(next)
  }, [])

  const value = useMemo<ThemeContextValue>(() => ({ theme, setTheme }), [theme, setTheme])

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}
