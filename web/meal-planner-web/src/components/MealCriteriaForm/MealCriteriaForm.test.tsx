import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import { MealCriteriaForm } from './MealCriteriaForm'

describe('MealCriteriaForm', () => {
  it('submits the selected criteria', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<MealCriteriaForm onSubmit={onSubmit} />)

    await user.selectOptions(screen.getByLabelText('Saison'), 'Winter')
    await user.selectOptions(screen.getByLabelText('Style'), 'Comforting')
    await user.click(screen.getByRole('button', { name: 'Générer des idées' }))

    expect(onSubmit).toHaveBeenCalledWith({
      season: 'Winter',
      style: 'Comforting',
      maxPrepTimeMinutes: null,
      includeIngredients: [],
      days: 7,
    })
  })

  it('submits null criteria when nothing is selected', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<MealCriteriaForm onSubmit={onSubmit} />)

    await user.click(screen.getByRole('button', { name: 'Générer des idées' }))

    expect(onSubmit).toHaveBeenCalledWith({
      season: null,
      style: null,
      maxPrepTimeMinutes: null,
      includeIngredients: [],
      days: 7,
    })
  })

  it('submits the available ingredients added by the cook', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<MealCriteriaForm onSubmit={onSubmit} />)

    const input = screen.getByLabelText('Ingrédients disponibles')
    await user.type(input, 'jambon{Enter}')
    await user.type(input, 'gruyère{Enter}')
    await user.click(screen.getByRole('button', { name: 'Générer des idées' }))

    expect(onSubmit).toHaveBeenCalledWith({
      season: null,
      style: null,
      maxPrepTimeMinutes: null,
      includeIngredients: ['jambon', 'gruyère'],
      days: 7,
    })
  })

  it('removes an ingredient from the list', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<MealCriteriaForm onSubmit={onSubmit} />)

    const input = screen.getByLabelText('Ingrédients disponibles')
    await user.type(input, 'jambon{Enter}')
    await user.type(input, 'gruyère{Enter}')
    await user.click(screen.getByRole('button', { name: 'Retirer jambon' }))
    await user.click(screen.getByRole('button', { name: 'Générer des idées' }))

    expect(onSubmit).toHaveBeenCalledWith({
      season: null,
      style: null,
      maxPrepTimeMinutes: null,
      includeIngredients: ['gruyère'],
      days: 7,
    })
  })

  it('submits the chosen number of days', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<MealCriteriaForm onSubmit={onSubmit} />)

    const days = screen.getByLabelText('Nombre de jours')
    await user.clear(days)
    await user.type(days, '5')
    await user.click(screen.getByRole('button', { name: 'Générer des idées' }))

    expect(onSubmit).toHaveBeenCalledWith({
      season: null,
      style: null,
      maxPrepTimeMinutes: null,
      includeIngredients: [],
      days: 5,
    })
  })
})
