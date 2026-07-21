import { createContext, useContext } from 'react'

import type { AuthenticatedUser, LoginInput, RegisterInput } from '../api/authTypes'

export type AuthStatus = 'authenticated' | 'anonymous'

export interface AuthContextValue {
  status: AuthStatus
  user: AuthenticatedUser | null
  login: (input: LoginInput) => Promise<void>
  register: (input: RegisterInput) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (context === null) {
    throw new Error('useAuth doit être utilisé à l’intérieur d’un AuthProvider')
  }
  return context
}
