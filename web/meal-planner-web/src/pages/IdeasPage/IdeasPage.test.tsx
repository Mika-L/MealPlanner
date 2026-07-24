import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { generateMealIdeas, replaceMealIdea } from '../../api/mealsClient'
import type { MealIdea } from '../../api/types'
import { IdeasPage } from './IdeasPage'

vi.mock('../../api/mealsClient', () => ({
  generateMealIdeas: vi.fn(),
  replaceMealIdea: vi.fn(),
}))

const mockedGenerate = vi.mocked(generateMealIdeas)
const mockedReplace = vi.mocked(replaceMealIdea)

function idea(day: number, name: string): MealIdea {
  return {
    day,
    id: name,
    name,
    description: '',
    prepTimeMinutes: 10,
    styles: [],
    ingredients: [],
    matchedIngredients: [],
  }
}

async function generate(ideas: MealIdea[]) {
  const user = userEvent.setup()
  mockedGenerate.mockResolvedValue(ideas)
  render(<IdeasPage />)
  await user.click(screen.getByRole('button', { name: 'Générer des idées' }))
  await screen.findByRole('heading', { name: ideas[0].name })
  return user
}

describe('IdeasPage', () => {
  beforeEach(() => {
    mockedGenerate.mockReset()
    mockedReplace.mockReset()
  })

  it('replaces an idea in place with the alternative returned by the API', async () => {
    const user = await generate([idea(1, 'Omelette'), idea(2, 'Soupe')])

    mockedReplace.mockResolvedValue(idea(1, 'Gratin'))
    await user.click(screen.getAllByRole('button', { name: /Remplacer/ })[0])

    expect(await screen.findByRole('heading', { name: 'Gratin' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Omelette' })).not.toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Soupe' })).toBeInTheDocument()

    // Le repas conservé (jour 2) verrouille ses ingrédients ; le repas remplacé n'est pas conservé.
    expect(mockedReplace).toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ day: 1, id: 'Omelette' }),
      [expect.objectContaining({ day: 2, id: 'Soupe' })],
    )
  })

  it('keeps the idea and warns when the API has no alternative', async () => {
    const user = await generate([idea(1, 'Omelette')])

    mockedReplace.mockResolvedValue(null)
    await user.click(screen.getByRole('button', { name: /Remplacer/ }))

    expect(
      await screen.findByText(/Aucune autre recette disponible pour le jour 1/),
    ).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Omelette' })).toBeInTheDocument()
  })
})
