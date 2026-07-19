import type { Recipe, RecipeInput } from './types'

export interface ListRecipesParams {
  search?: string
  page?: number
  pageSize?: number
}

export interface RecipePage {
  recipes: Recipe[]
  page: number
  pageSize: number
  total: number
}

interface ListRecipesResponse {
  meals: Recipe[]
  page: number
  pageSize: number
  total: number
}

export async function listRecipes(
  params: ListRecipesParams = {},
  signal?: AbortSignal,
): Promise<RecipePage> {
  const query = new URLSearchParams()
  if (params.search?.trim()) {
    query.set('search', params.search.trim())
  }
  if (params.page) {
    query.set('page', String(params.page))
  }
  if (params.pageSize) {
    query.set('pageSize', String(params.pageSize))
  }

  const queryString = query.toString()
  const response = await fetch(`/api/meals${queryString ? `?${queryString}` : ''}`, { signal })

  if (!response.ok) {
    throw new Error(`Le chargement des recettes a échoué (HTTP ${response.status}).`)
  }

  const payload = (await response.json()) as ListRecipesResponse
  return {
    recipes: payload.meals,
    page: payload.page,
    pageSize: payload.pageSize,
    total: payload.total,
  }
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
