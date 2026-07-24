import { useState } from 'react'

import { generateMealIdeas, replaceMealIdea } from '../../api/mealsClient'
import type { MealCriteria, MealIdea } from '../../api/types'
import { MealCriteriaForm } from '../../components/MealCriteriaForm/MealCriteriaForm'

export function IdeasPage() {
  const [ideas, setIdeas] = useState<MealIdea[]>([])
  const [criteria, setCriteria] = useState<MealCriteria | null>(null)
  const [status, setStatus] = useState<'idle' | 'loading' | 'error'>('idle')
  const [hasSearched, setHasSearched] = useState(false)
  const [replacingDay, setReplacingDay] = useState<number | null>(null)
  const [notice, setNotice] = useState<string | null>(null)
  // Recettes déjà proposées sur l'ensemble du planning (tous jours confondus) : l'API en pioche une
  // nouvelle à chaque remplacement, si bien qu'une recette écartée ne réapparaît pas — ni le même jour,
  // ni un autre — tant que le pool éligible n'est pas épuisé.
  const [seen, setSeen] = useState<string[]>([])

  const handleSubmit = async (submitted: MealCriteria) => {
    setStatus('loading')
    setHasSearched(true)
    setNotice(null)
    try {
      const generated = await generateMealIdeas(submitted)
      setIdeas(generated)
      setSeen(generated.map((idea) => idea.id))
      setCriteria(submitted)
      setStatus('idle')
    } catch {
      setStatus('error')
    }
  }

  // Remplace une idée par une autre recette : on conserve les autres repas du planning (leurs
  // ingrédients du frigo restent verrouillés) et on demande à l'API une alternative respectant les critères.
  const handleReplace = async (target: MealIdea) => {
    if (criteria === null) {
      return
    }

    setReplacingDay(target.day)
    setNotice(null)
    try {
      const kept = ideas.filter((idea) => idea.day !== target.day)
      const replacement = await replaceMealIdea(criteria, target, kept, seen)
      if (replacement === null) {
        setNotice(`Aucune autre recette disponible pour le jour ${target.day}.`)
        return
      }
      const nextIdeas = ideas.map((idea) => (idea.day === target.day ? replacement : idea))
      setIdeas(nextIdeas)
      // Une recette déjà vue qui ressort signale que le pool est épuisé : on repart de l'écran courant
      // pour reparcourir toutes les alternatives avant de boucler à nouveau.
      setSeen(
        seen.includes(replacement.id)
          ? nextIdeas.map((idea) => idea.id)
          : [...seen, replacement.id],
      )
    } catch {
      setNotice('Le remplacement a échoué. Réessaie.')
    } finally {
      setReplacingDay(null)
    }
  }

  return (
    <>
      <h1>Idées de repas</h1>
      <p className="app__lead">Choisis tes critères, on te propose des repas.</p>

      <MealCriteriaForm onSubmit={handleSubmit} />

      {status === 'loading' && <p role="status">Recherche en cours…</p>}
      {status === 'error' && (
        <p role="alert" className="app__error">
          Une erreur est survenue. Vérifie que l'API et la base de données sont démarrées.
        </p>
      )}

      {notice !== null && (
        <p role="status" className="app__notice">
          {notice}
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
            <div className="idea-card__actions">
              <button
                type="button"
                onClick={() => void handleReplace(idea)}
                disabled={replacingDay !== null}
              >
                {replacingDay === idea.day ? 'Remplacement…' : '↻ Remplacer'}
              </button>
            </div>
          </li>
        ))}
      </ol>
    </>
  )
}
