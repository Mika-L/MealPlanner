import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'

import { ThemeProvider } from '../../theme'
import { PreferencesPage } from './PreferencesPage'

function renderPage() {
  return render(
    <ThemeProvider>
      <PreferencesPage />
    </ThemeProvider>,
  )
}

describe('PreferencesPage', () => {
  beforeEach(() => {
    localStorage.clear()
    delete document.documentElement.dataset.palette
  })

  it('offers both the Stone and Coral themes', () => {
    renderPage()

    expect(screen.getByRole('radio', { name: /Stone/ })).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: /Corail/ })).toBeInTheDocument()
  })

  it('selects Stone by default', () => {
    renderPage()

    expect(screen.getByRole('radio', { name: /Stone/ })).toBeChecked()
    expect(screen.getByRole('radio', { name: /Corail/ })).not.toBeChecked()
    expect(document.documentElement.dataset.palette).toBe('stone')
  })

  it('applies and persists the Coral theme when chosen', async () => {
    const user = userEvent.setup()
    renderPage()

    await user.click(screen.getByRole('radio', { name: /Corail/ }))

    expect(screen.getByRole('radio', { name: /Corail/ })).toBeChecked()
    expect(document.documentElement.dataset.palette).toBe('coral')
    expect(localStorage.getItem('mealplanner-theme')).toBe('coral')
  })
})
