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
  includeIngredients: string[]
  days: number
}

export interface MealIdea {
  day: number
  id: string
  name: string
  description: string
  prepTimeMinutes: number
  styles: MealStyleName[]
  ingredients: string[]
  matchedIngredients: string[]
}

export interface Recipe {
  id: string
  name: string
  description: string
  seasons: SeasonName[]
  styles: MealStyleName[]
  prepTimeMinutes: number
  ingredients: string[]
}

// Charge utile envoyée à l'API pour créer ou modifier une recette (sans identifiant).
export type RecipeInput = Omit<Recipe, 'id'>
