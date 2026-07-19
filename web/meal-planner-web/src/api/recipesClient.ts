import type { Recipe, RecipeInput } from './types'

interface ListRecipesResponse {
  meals: Recipe[]
}

export async function listRecipes(signal?: AbortSignal): Promise<Recipe[]> {
  const response = await fetch('/api/meals', { signal })

  if (!response.ok) {
    throw new Error(`Le chargement des recettes a échoué (HTTP ${response.status}).`)
  }

  const payload = (await response.json()) as ListRecipesResponse
  return payload.meals
}

export async function createRecipe(input: RecipeInput, signal?: AbortSignal): Promise<string> {
  const response = await fetch('/api/meals', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
    signal,
  })

  if (!response.ok) {
    throw new Error(`La création de la recette a échoué (HTTP ${response.status}).`)
  }

  const payload = (await response.json()) as { id: string }
  return payload.id
}

export async function updateRecipe(
  id: string,
  input: RecipeInput,
  signal?: AbortSignal,
): Promise<void> {
  const response = await fetch(`/api/meals/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
    signal,
  })

  if (!response.ok) {
    throw new Error(`La modification de la recette a échoué (HTTP ${response.status}).`)
  }
}

export async function deleteRecipe(id: string, signal?: AbortSignal): Promise<void> {
  const response = await fetch(`/api/meals/${id}`, { method: 'DELETE', signal })

  if (!response.ok) {
    throw new Error(`La suppression de la recette a échoué (HTTP ${response.status}).`)
  }
}
