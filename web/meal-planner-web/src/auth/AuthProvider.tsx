import { useCallback, useEffect, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'

import type { AuthenticatedUser, LoginInput, RegisterInput } from '../api/authTypes'
import * as authClient from '../api/authClient'
import { AuthContext, type AuthContextValue } from './AuthContext'
import { clearSession, getSession, setSession, subscribe } from './tokenStore'

export function AuthProvider({ children }: PropsWithChildren) {
  // Restauration optimiste : si une session est persistée, on démarre authentifié
  // sans écran de chargement, puis on la valide en arrière-plan.
  const [user, setUser] = useState<AuthenticatedUser | null>(() => getSession()?.user ?? null)

  // Reflète toute mutation externe de la session (refresh échoué → purge → déconnexion).
  useEffect(() => {
    return subscribe(() => setUser(getSession()?.user ?? null))
  }, [])

  // Valide la session restaurée au démarrage (refresh transparent géré par apiFetch).
  useEffect(() => {
    if (!getSession()) {
      return
    }
    const controller = new AbortController()
    authClient
      .getCurrentUser(controller.signal)
      .then((fresh) => setUser(fresh))
      .catch(() => {
        // Un 401 non rattrapable a déjà purgé la session ; toute autre erreur
        // (réseau, annulation) est transitoire et laisse la session en place.
      })
    return () => controller.abort()
  }, [])

  const login = useCallback(async (input: LoginInput) => {
    setSession(await authClient.login(input))
  }, [])

  const register = useCallback(async (input: RegisterInput) => {
    setSession(await authClient.register(input))
  }, [])

  const loginWithGoogle = useCallback(async (idToken: string) => {
    setSession(await authClient.loginWithGoogle(idToken))
  }, [])

  const logout = useCallback(() => {
    clearSession()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      status: user ? 'authenticated' : 'anonymous',
      user,
      login,
      register,
      loginWithGoogle,
      logout,
    }),
    [user, login, register, loginWithGoogle, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
