import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import type { RecipeInput } from '../../api/types'
import { RecipeForm } from './RecipeForm'

describe('RecipeForm', () => {
  it('submits a new recipe with its seasons, styles and ingredients', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    render(<RecipeForm submitLabel="Créer la recette" onSubmit={onSubmit} onCancel={vi.fn()} />)

    await user.type(screen.getByLabelText('Nom'), 'Ratatouille')
    await user.type(screen.getByLabelText('Description'), 'Mijoté de légumes.')
    await user.click(screen.getByRole('checkbox', { name: 'Summer' }))
    await user.click(screen.getByRole('checkbox', { name: 'Healthy' }))

    const prepTime = screen.getByLabelText('Temps de préparation (min)')
    await user.clear(prepTime)
    await user.type(prepTime, '60')

    await user.type(screen.getByLabelText('Ingrédients'), 'courgette{Enter}')
    await user.click(screen.getByRole('button', { name: 'Créer la recette' }))

    expect(onSubmit).toHaveBeenCalledWith({
      name: 'Ratatouille',
      description: 'Mijoté de légumes.',
      seasons: ['Summer'],
      styles: ['Healthy'],
      prepTimeMinutes: 60,
      ingredients: ['courgette'],
    } satisfies RecipeInput)
  })

  it('pre-fills the fields when editing an existing recipe', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    const initialValue: RecipeInput = {
      name: 'Omelette',
      description: 'Rapide',
      seasons: ['Spring'],
      styles: ['Quick'],
      prepTimeMinutes: 10,
      ingredients: ['œuf'],
    }
    render(
      <RecipeForm
        initialValue={initialValue}
        submitLabel="Enregistrer"
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByLabelText('Nom')).toHaveValue('Omelette')
    expect(screen.getByRole('checkbox', { name: 'Spring' })).toBeChecked()
    expect(screen.getByRole('checkbox', { name: 'Quick' })).toBeChecked()

    await user.click(screen.getByRole('button', { name: 'Enregistrer' }))

    expect(onSubmit).toHaveBeenCalledWith(initialValue)
  })

  it('calls onCancel without submitting', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()
    const onCancel = vi.fn()
    render(<RecipeForm submitLabel="Créer la recette" onSubmit={onSubmit} onCancel={onCancel} />)

    await user.click(screen.getByRole('button', { name: 'Annuler' }))

    expect(onCancel).toHaveBeenCalled()
    expect(onSubmit).not.toHaveBeenCalled()
  })
})
