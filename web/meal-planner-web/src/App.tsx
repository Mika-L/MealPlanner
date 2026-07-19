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

      <ol className="app__results">
        {ideas.map((idea) => (
          <li key={idea.id} className="app__card">
            <p className="app__day">Jour {idea.day}</p>
            <h2>{idea.name}</h2>
            {idea.styles.length > 0 && (
              <ul className="app__styles" aria-label="Styles">
                {idea.styles.map((style) => (
                  <li key={style} className="app__style">
                    {style}
                  </li>
                ))}
              </ul>
            )}
            <p>{idea.description}</p>
            <p className="app__meta">⏱ {idea.prepTimeMinutes} min</p>
            {idea.ingredients.length > 0 && (
              <p className="app__meta">🧺 {idea.ingredients.join(', ')}</p>
            )}
            {idea.matchedIngredients.length > 0 && (
              <p className="app__meta">✅ Utilise : {idea.matchedIngredients.join(', ')}</p>
            )}
          </li>
        ))}
      </ol>
    </main>
  )
}

export default App
