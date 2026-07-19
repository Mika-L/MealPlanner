import { createContext, useContext } from 'react'

import type { ThemeId } from './themes'

export interface ThemeContextValue {
  theme: ThemeId
  setTheme: (theme: ThemeId) => void
}

export const ThemeContext = createContext<ThemeContextValue | null>(null)

export function useTheme(): ThemeContextValue {
  const context = useContext(ThemeContext)
  if (context === null) {
    throw new Error('useTheme doit être utilisé à l’intérieur d’un ThemeProvider')
  }
  return context
}
