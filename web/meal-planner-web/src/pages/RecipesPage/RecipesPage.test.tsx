import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { RecipePage } from '../../api/recipesClient'
import { listRecipes } from '../../api/recipesClient'
import type { Recipe } from '../../api/types'
import { RecipesPage } from './RecipesPage'

vi.mock('../../api/recipesClient', () => ({
  listRecipes: vi.fn(),
  createRecipe: vi.fn(),
  updateRecipe: vi.fn(),
  deleteRecipe: vi.fn(),
}))

const mockedList = vi.mocked(listRecipes)

function recipe(name: string): Recipe {
  return {
    id: name,
    name,
    description: '',
    seasons: [],
    styles: [],
    prepTimeMinutes: 10,
    ingredients: [],
  }
}

function pageOf(recipes: Recipe[], total: number, page = 1): RecipePage {
  return { recipes, total, page, pageSize: 24 }
}

describe('RecipesPage', () => {
  beforeEach(() => {
    mockedList.mockReset()
  })

  it('lists the recipes returned by the API with a count', async () => {
    mockedList.mockResolvedValue(pageOf([recipe('Avocado toast')], 1))

    render(<RecipesPage />)

    expect(await screen.findByRole('heading', { name: 'Avocado toast' })).toBeInTheDocument()
    expect(screen.getByText('1 recette')).toBeInTheDocument()
  })

  it('asks the API for a filtered page when the user searches', async () => {
    const user = userEvent.setup()
    mockedList.mockResolvedValue(pageOf([recipe('Avocado toast'), recipe('Zurek')], 2))

    render(<RecipesPage />)
    await screen.findByRole('heading', { name: 'Zurek' })

    mockedList.mockResolvedValue(pageOf([recipe('Zurek')], 1))
    await user.type(screen.getByLabelText('Rechercher une recette'), 'zurek')

    await waitFor(() =>
      expect(mockedList).toHaveBeenCalledWith(
        expect.objectContaining({ search: 'zurek', page: 1 }),
        expect.anything(),
      ),
    )
    expect(await screen.findByText('1 résultat')).toBeInTheDocument()
  })

  it('appends the next page when the user clicks "Afficher plus"', async () => {
    const user = userEvent.setup()
    mockedList.mockResolvedValueOnce(pageOf([recipe('A'), recipe('B')], 3, 1))

    render(<RecipesPage />)
    await screen.findByRole('heading', { name: 'A' })

    mockedList.mockResolvedValueOnce(pageOf([recipe('C')], 3, 2))
    await user.click(screen.getByRole('button', { name: /Afficher plus/ }))

    expect(await screen.findByRole('heading', { name: 'C' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'A' })).toBeInTheDocument()
    expect(mockedList).toHaveBeenLastCalledWith(
      expect.objectContaining({ page: 2 }),
      expect.anything(),
    )
  })
})
