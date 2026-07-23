import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it } from 'vitest'

import { AuthProvider } from '../../auth'
import { ThemeProvider } from '../../theme'
import { PreferencesPage } from './PreferencesPage'

// Anonyme (aucune session en localStorage) : le thème reste purement local, pas de sync serveur.
function renderPage() {
  return render(
    <AuthProvider>
      <ThemeProvider>
        <PreferencesPage />
      </ThemeProvider>
    </AuthProvider>,
  )
}

describe('PreferencesPage', () => {
  beforeEach(() => {
    localStorage.clear()
    delete document.documentElement.dataset.palette
  })

  it('offers both the Sauge and Coral themes', () => {
    renderPage()

    expect(screen.getByRole('radio', { name: /Sauge/ })).toBeInTheDocument()
    expect(screen.getByRole('radio', { name: /Corail/ })).toBeInTheDocument()
  })

  it('selects Sauge by default', () => {
    renderPage()

    expect(screen.getByRole('radio', { name: /Sauge/ })).toBeChecked()
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
