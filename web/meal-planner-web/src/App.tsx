import { NavLink, Route, Routes, useNavigate } from 'react-router'

import { useAuth } from './auth'
import { RequireAuth } from './components/RequireAuth/RequireAuth'
import { AuthPage } from './pages/AuthPage/AuthPage'
import { IdeasPage } from './pages/IdeasPage/IdeasPage'
import { PreferencesPage } from './pages/PreferencesPage/PreferencesPage'
import { RecipesPage } from './pages/RecipesPage/RecipesPage'
import './App.css'

function App() {
  const { status, user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="app">
      <header className="app__brand">
        <a className="app__logo" href="/" aria-label="Accueil MealPlanner">
          <span className="app__logo-mark" aria-hidden="true">
            🍽️
          </span>
          <span className="app__logo-text">
            Meal<span className="app__logo-accent">Planner</span>
          </span>
        </a>

        {status === 'authenticated' && (
          <>
            <nav className="app__nav" aria-label="Navigation principale">
              <NavLink to="/" end>
                Idées de repas
              </NavLink>
              <NavLink to="/recettes">Mes recettes</NavLink>
              <NavLink to="/preferences">Préférences</NavLink>
            </nav>

            <div className="app__account">
              <span className="app__account-name">{user?.displayName || user?.email}</span>
              <button type="button" className="app__logout" onClick={handleLogout}>
                Déconnexion
              </button>
            </div>
          </>
        )}
      </header>

      <main className="app__main">
        <Routes>
          <Route path="/login" element={<AuthPage />} />
          <Route
            path="/"
            element={
              <RequireAuth>
                <IdeasPage />
              </RequireAuth>
            }
          />
          <Route
            path="/recettes"
            element={
              <RequireAuth>
                <RecipesPage />
              </RequireAuth>
            }
          />
          <Route
            path="/preferences"
            element={
              <RequireAuth>
                <PreferencesPage />
              </RequireAuth>
            }
          />
        </Routes>
      </main>
    </div>
  )
}

export default App
