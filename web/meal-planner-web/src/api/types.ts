// Contrat aligné sur l'API MealPlanner (module Meals)

export const seasons = ['Spring', 'Summer', 'Autumn', 'Winter'] as const
export type SeasonName = (typeof seasons)[number]

export const mealStyles = [
  'Healthy',
  'Comforting',
  'Quick',
  'Festive',
  'Light',
  'Gourmet',
] as const
export type MealStyleName = (typeof mealStyles)[number]

export interface MealCriteria {
  season: SeasonName | null
  style: MealStyleName | null
  maxPrepTimeMinutes: number | null
}

export interface MealIdea {
  id: string
  name: string
  description: string
  prepTimeMinutes: number
  ingredients: string[]
}
