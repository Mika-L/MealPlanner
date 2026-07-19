import type { PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router'

import { useAuth } from '../../auth'

/** Protège une route : redirige vers /login en conservant la destination visée. */
export function RequireAuth({ children }: PropsWithChildren) {
  const { status } = useAuth()
  const location = useLocation()

  if (status === 'anonymous') {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return children
}
