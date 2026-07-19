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
    })
  })
})
