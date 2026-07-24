import type { MealCriteria, MealIdea } from './types'
import { apiFetch } from './client'

interface GenerateMealIdeasResponse {
  ideas: MealIdea[]
}

export async function generateMealIdeas(
  criteria: MealCriteria,
  signal?: AbortSignal,
): Promise<MealIdea[]> {
  const response = await apiFetch('/api/meals/ideas', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      season: criteria.season,
      styles: criteria.style,
      maxPrepTimeMinutes: criteria.maxPrepTimeMinutes,
      includeIngredients: criteria.includeIngredients,
      days: criteria.days,
    }),
    signal,
  })

  if (!response.ok) {
    throw new Error(`La génération d'idées a échoué (HTTP ${response.status}).`)
  }

  const payload = (await response.json()) as GenerateMealIdeasResponse
  return payload.ideas
}

interface ReplaceMealIdeaResponse {
  meal: MealIdea
}

/**
 * Remplace une idée du planning par une autre recette respectant les critères de la génération.
 * Les repas conservés (`kept`) verrouillent leurs ingrédients du frigo (« un ingrédient = un repas »)
 * et ne sont jamais repiochés, pas plus que l'idée remplacée. `seen` liste les recettes déjà proposées
 * sur tout le planning (tous jours confondus) : l'API en pioche une nouvelle à chaque appel (une autre à
 * chaque fois, sans réapparaître sur un autre jour). Renvoie `null` quand aucune alternative n'existe (HTTP 404).
 */
export async function replaceMealIdea(
  criteria: MealCriteria,
  replaced: MealIdea,
  kept: MealIdea[],
  seen: string[],
  signal?: AbortSignal,
): Promise<MealIdea | null> {
  const response = await apiFetch('/api/meals/ideas/replace', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      season: criteria.season,
      styles: criteria.style,
      maxPrepTimeMinutes: criteria.maxPrepTimeMinutes,
      includeIngredients: criteria.includeIngredients,
      day: replaced.day,
      replacedMealId: replaced.id,
      keptMealIds: kept.map((idea) => idea.id),
      seenMealIds: seen,
    }),
    signal,
  })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Le remplacement a échoué (HTTP ${response.status}).`)
  }

  const payload = (await response.json()) as ReplaceMealIdeaResponse
  return payload.meal
}
