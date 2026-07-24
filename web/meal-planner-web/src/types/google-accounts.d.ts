// Typages minimaux de Google Identity Services (script accounts.google.com/gsi/client),
// limités à ce qu'utilise GoogleSignInButton : initialisation + rendu du bouton officiel.
// Docs : https://developers.google.com/identity/gsi/web/reference/js-reference

interface GoogleCredentialResponse {
  /** JWT signé par Google : c'est l'id_token à transmettre au back (/api/auth/google). */
  credential: string
  select_by?: string
}

interface GoogleIdConfiguration {
  client_id: string
  callback: (response: GoogleCredentialResponse) => void
  auto_select?: boolean
  cancel_on_tap_outside?: boolean
}

interface GoogleButtonConfiguration {
  type?: 'standard' | 'icon'
  theme?: 'outline' | 'filled_blue' | 'filled_black'
  size?: 'large' | 'medium' | 'small'
  text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin'
  shape?: 'rectangular' | 'pill' | 'circle' | 'square'
  logo_alignment?: 'left' | 'center'
  width?: number
  locale?: string
}

interface GoogleAccountsId {
  initialize: (config: GoogleIdConfiguration) => void
  renderButton: (parent: HTMLElement, options: GoogleButtonConfiguration) => void
  cancel: () => void
}

interface Window {
  google?: {
    accounts: {
      id: GoogleAccountsId
    }
  }
}
