import { useState } from 'react'

import { generateMealIdeas } from './api/mealsClient'
import type { MealCriteria, MealIdea } from './api/types'
import { MealCriteriaForm } from './components/MealCriteriaForm/MealCriteriaForm'
import './App.css'

function App() {
  const [ideas, setIdeas] = useState<MealIdea[]>([])
  const [status, setStatus] = useState<'idle' | 'loading' | 'error'>('idle')
  const [hasSearched, setHasSearched] = useState(false)

  const handleSubmit = async (criteria: MealCriteria) => {
    setStatus('loading')
    setHasSearched(true)
    try {
      setIdeas(await generateMealIdeas(criteria))
      setStatus('idle')
    } catch {
      setStatus('error')
    }
  }

  return (
    <main className="app">
      <h1>Idées de repas</h1>
      <p className="app__lead">Choisis tes critères, on te propose des repas.</p>

      <MealCriteriaForm onSubmit={handleSubmit} />

      {status === 'loading' && <p role="status">Recherche en cours…</p>}
      {status === 'error' && (
        <p role="alert" className="app__error">
          Une erreur est survenue. Vérifie que l'API et la base de données sont démarrées.
        </p>
      )}

      {status === 'idle' && hasSearched && ideas.length === 0 && (
        <p role="status">Aucune idée ne correspond à ces critères.</p>
      )}

      <ul className="app__results">
        {ideas.map((idea) => (
          <li key={idea.id} className="app__card">
            <h2>{idea.name}</h2>
            <p>{idea.description}</p>
            <p className="app__meta">⏱ {idea.prepTimeMinutes} min</p>
            {idea.ingredients.length > 0 && (
              <p className="app__meta">🧺 {idea.ingredients.join(', ')}</p>
            )}
          </li>
        ))}
      </ul>
    </main>
  )
}

export default App
