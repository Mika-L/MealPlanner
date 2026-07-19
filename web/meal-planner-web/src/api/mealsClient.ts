import type { MealCriteria, MealIdea } from './types'

interface GenerateMealIdeasResponse {
  ideas: MealIdea[]
}

export async function generateMealIdeas(
  criteria: MealCriteria,
  signal?: AbortSignal,
): Promise<MealIdea[]> {
  const response = await fetch('/api/meals/ideas', {
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
