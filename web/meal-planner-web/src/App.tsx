import { NavLink, Route, Routes } from 'react-router'

import { IdeasPage } from './pages/IdeasPage/IdeasPage'
import { RecipesPage } from './pages/RecipesPage/RecipesPage'
import './App.css'

function App() {
  return (
    <div className="app">
      <nav className="app__nav" aria-label="Navigation principale">
        <NavLink to="/" end>
          Idées de repas
        </NavLink>
        <NavLink to="/recettes">Mes recettes</NavLink>
      </nav>

      <main>
        <Routes>
          <Route path="/" element={<IdeasPage />} />
          <Route path="/recettes" element={<RecipesPage />} />
        </Routes>
      </main>
    </div>
  )
}

export default App
