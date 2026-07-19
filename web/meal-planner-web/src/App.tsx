import { NavLink, Route, Routes } from 'react-router'

import { IdeasPage } from './pages/IdeasPage/IdeasPage'
import { RecipesPage } from './pages/RecipesPage/RecipesPage'
import './App.css'

function App() {
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
        <nav className="app__nav" aria-label="Navigation principale">
          <NavLink to="/" end>
            Idées de repas
          </NavLink>
          <NavLink to="/recettes">Mes recettes</NavLink>
        </nav>
      </header>

      <main className="app__main">
        <Routes>
          <Route path="/" element={<IdeasPage />} />
          <Route path="/recettes" element={<RecipesPage />} />
        </Routes>
      </main>
    </div>
  )
}

export default App
