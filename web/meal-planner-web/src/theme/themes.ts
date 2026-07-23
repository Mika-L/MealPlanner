export type ThemeId = 'stone' | 'coral'

export interface ThemeOption {
  id: ThemeId
  label: string
  description: string
  /** Aperçu de la palette : accent, secondaire, fond, encre. */
  swatches: [string, string, string, string]
}

export const themes: ThemeOption[] = [
  {
    id: 'stone',
    label: 'Sauge',
    description: 'Douce et lumineuse, dans les teintes de notre cuisine : sauge, chêne clair et laiton.',
    swatches: ['#7f9a80', '#b8935a', '#f7f5f0', '#46503c'],
  },
  {
    id: 'coral',
    label: 'Corail',
    description: 'La palette pétillante d’origine : corail, rose et une pointe de violet.',
    swatches: ['#ff4d6d', '#7c5cff', '#fdf6f3', '#b81d3f'],
  },
]

export const defaultTheme: ThemeId = 'stone'

export const themeStorageKey = 'mealplanner-theme'

export function isThemeId(value: unknown): value is ThemeId {
  return value === 'stone' || value === 'coral'
}
