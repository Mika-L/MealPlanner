import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { MealCriteria, MealIdea } from './types'
import { apiFetch } from './client'
import { replaceMealIdea } from './mealsClient'

vi.mock('./client', () => ({ apiFetch: vi.fn() }))

const mockedFetch = vi.mocked(apiFetch)

function criteria(overrides: Partial<MealCriteria> = {}): MealCriteria {
  return {
    season: 'Winter',
    style: 'Comforting',
    maxPrepTimeMinutes: 45,
    includeIngredients: ['tomate'],
    days: 7,
    ...overrides,
  }
}

function idea(day: number, id: string): MealIdea {
  return {
    day,
    id,
    name: id,
    description: '',
    prepTimeMinutes: 10,
    styles: [],
    ingredients: [],
    matchedIngredients: [],
  }
}

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}

describe('replaceMealIdea', () => {
  beforeEach(() => {
    mockedFetch.mockReset()
  })

  it('posts the criteria, the replaced day, the kept meal ids and the recipes already seen', async () => {
    const replacement = idea(1, 'Gratin')
    mockedFetch.mockResolvedValue(jsonResponse(200, { meal: replacement }))

    const result = await replaceMealIdea(criteria(), idea(1, 'Omelette'), [idea(2, 'Soupe')], [
      'Omelette',
      'Quiche',
    ])

    expect(result).toEqual(replacement)
    expect(mockedFetch.mock.calls[0][0]).toBe('/api/meals/ideas/replace')
    const body = JSON.parse(mockedFetch.mock.calls[0][1]?.body as string)
    expect(body).toEqual({
      season: 'Winter',
      styles: 'Comforting',
      maxPrepTimeMinutes: 45,
      includeIngredients: ['tomate'],
      day: 1,
      replacedMealId: 'Omelette',
      keptMealIds: ['Soupe'],
      seenMealIds: ['Omelette', 'Quiche'],
    })
  })

  it('returns null when the API has no alternative (404)', async () => {
    mockedFetch.mockResolvedValue(jsonResponse(404, {}))

    const result = await replaceMealIdea(criteria(), idea(1, 'Omelette'), [], ['Omelette'])

    expect(result).toBeNull()
  })

  it('throws on an unexpected error status', async () => {
    mockedFetch.mockResolvedValue(jsonResponse(500, {}))

    await expect(replaceMealIdea(criteria(), idea(1, 'Omelette'), [], ['Omelette'])).rejects.toThrow()
  })
})
