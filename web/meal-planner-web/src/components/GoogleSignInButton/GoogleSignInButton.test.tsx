import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

// L'ID client est capturé à l'import du module ; on stubbe l'env puis on ré-importe
// à chaud pour couvrir les deux branches (configuré / absent).
async function importButton() {
  vi.resetModules()
  return (await import('./GoogleSignInButton')).GoogleSignInButton
}

describe('GoogleSignInButton', () => {
  afterEach(() => {
    vi.unstubAllEnvs()
    vi.resetModules()
    delete (window as { google?: unknown }).google
  })

  it('shows a disabled fallback when no Google client id is configured', async () => {
    vi.stubEnv('VITE_GOOGLE_CLIENT_ID', '')
    const GoogleSignInButton = await importButton()

    render(<GoogleSignInButton onCredential={vi.fn()} />)

    expect(screen.getByRole('button', { name: 'Continuer avec Google' })).toBeDisabled()
  })

  it('initializes Google and forwards the id_token from the credential callback', async () => {
    vi.stubEnv('VITE_GOOGLE_CLIENT_ID', 'client-123')

    let credentialCallback: ((response: { credential: string }) => void) | undefined
    const initialize = vi.fn((config: { callback: (r: { credential: string }) => void }) => {
      credentialCallback = config.callback
    })
    const renderButton = vi.fn()
    ;(window as { google?: unknown }).google = {
      accounts: { id: { initialize, renderButton, cancel: vi.fn() } },
    }

    const GoogleSignInButton = await importButton()
    const onCredential = vi.fn()
    render(<GoogleSignInButton onCredential={onCredential} />)

    await waitFor(() =>
      expect(initialize).toHaveBeenCalledWith(
        expect.objectContaining({ client_id: 'client-123' }),
      ),
    )
    expect(renderButton).toHaveBeenCalled()

    credentialCallback?.({ credential: 'id-token-xyz' })
    expect(onCredential).toHaveBeenCalledWith('id-token-xyz')
  })
})
