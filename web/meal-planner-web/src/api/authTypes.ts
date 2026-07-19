// Contrat aligné sur l'API MealPlanner (module Identity, AuthResult / AuthenticatedUser).

export interface AuthenticatedUser {
  id: string
  email: string
  displayName: string | null
}

export interface AuthResult {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: AuthenticatedUser
}

export interface RegisterInput {
  email: string
  password: string
  displayName?: string
}

export interface LoginInput {
  email: string
  password: string
}
