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
    label: 'Stone',
    description: 'Greige chaud et minéral, dans la teinte des façades de cuisine Plum Living.',
    swatches: ['#8c8273', '#b1936a', '#f6f2ec', '#57503f'],
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
