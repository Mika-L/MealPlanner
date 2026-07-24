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
    // Les idées initiales de tout le planning sont envoyées comme « déjà vues » pour ne pas être repiochées.
    expect(mockedReplace).toHaveBeenCalledWith(
      expect.anything(),
      expect.objectContaining({ day: 1, id: 'Omelette' }),
      [expect.objectContaining({ day: 2, id: 'Soupe' })],
      ['Omelette', 'Soupe'],
    )
  })

  it('shares the seen history across days so a discarded recipe never resurfaces on another day', async () => {
    const user = await generate([idea(1, 'Omelette'), idea(2, 'Soupe')])

    // On remplace l'idée du jour 1 : « Omelette » est écartée.
    mockedReplace.mockResolvedValueOnce(idea(1, 'Gratin'))
    await user.click(screen.getAllByRole('button', { name: /Remplacer/ })[0])
    await screen.findByRole('heading', { name: 'Gratin' })

    // En remplaçant le jour 2, l'historique commun exclut « Omelette » (écartée au jour 1) en plus
    // des idées initiales : une recette abandonnée sur un jour ne réapparaît pas sur un autre.
    mockedReplace.mockResolvedValueOnce(idea(2, 'Curry'))
    await user.click(screen.getAllByRole('button', { name: /Remplacer/ })[1])
    await screen.findByRole('heading', { name: 'Curry' })

    expect(mockedReplace).toHaveBeenLastCalledWith(
      expect.anything(),
      expect.objectContaining({ day: 2, id: 'Soupe' }),
      [expect.objectContaining({ day: 1, id: 'Gratin' })],
      ['Omelette', 'Soupe', 'Gratin'],
    )
  })

  it('accumulates the seen recipes so each replacement asks for a new one, then restarts the cycle', async () => {
    const user = await generate([idea(1, 'Omelette')])

    mockedReplace.mockResolvedValueOnce(idea(1, 'Gratin'))
    await user.click(screen.getByRole('button', { name: /Remplacer/ }))
    await screen.findByRole('heading', { name: 'Gratin' })

    // 2e remplacement : l'historique inclut désormais l'idée initiale ET la précédente alternative.
    mockedReplace.mockResolvedValueOnce(idea(1, 'Quiche'))
    await user.click(screen.getByRole('button', { name: /Remplacer/ }))
    await screen.findByRole('heading', { name: 'Quiche' })
    expect(mockedReplace).toHaveBeenLastCalledWith(
      expect.anything(),
      expect.objectContaining({ id: 'Gratin' }),
      [],
      ['Omelette', 'Gratin'],
    )

    // Le pool est épuisé : l'API renvoie une recette déjà vue → l'historique repart de zéro.
    mockedReplace.mockResolvedValueOnce(idea(1, 'Omelette'))
    await user.click(screen.getByRole('button', { name: /Remplacer/ }))
    await screen.findByRole('heading', { name: 'Omelette' })

    mockedReplace.mockResolvedValueOnce(idea(1, 'Gratin'))
    await user.click(screen.getByRole('button', { name: /Remplacer/ }))
    await screen.findByRole('heading', { name: 'Gratin' })
    expect(mockedReplace).toHaveBeenLastCalledWith(
      expect.anything(),
      expect.objectContaining({ id: 'Omelette' }),
      [],
      ['Omelette'],
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
