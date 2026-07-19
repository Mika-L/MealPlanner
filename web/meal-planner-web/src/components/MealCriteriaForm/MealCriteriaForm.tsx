import { useState } from 'react'

import {
  mealStyles,
  seasons,
  type MealCriteria,
  type MealStyleName,
  type SeasonName,
} from '../../api/types'

interface MealCriteriaFormProps {
  onSubmit: (criteria: MealCriteria) => void
}

export function MealCriteriaForm({ onSubmit }: MealCriteriaFormProps) {
  const [season, setSeason] = useState<SeasonName | ''>('')
  const [style, setStyle] = useState<MealStyleName | ''>('')
  const [days, setDays] = useState('7')
  const [ingredients, setIngredients] = useState<string[]>([])
  const [draft, setDraft] = useState('')

  const addIngredient = () => {
    const value = draft.trim()
    if (value === '' || ingredients.includes(value)) {
      setDraft('')
      return
    }
    setIngredients([...ingredients, value])
    setDraft('')
  }

  const removeIngredient = (value: string) => {
    setIngredients(ingredients.filter((ingredient) => ingredient !== value))
  }

  const handleIngredientKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault()
      addIngredient()
    }
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    onSubmit({
      season: season === '' ? null : season,
      style: style === '' ? null : style,
      maxPrepTimeMinutes: null,
      includeIngredients: ingredients,
      days: Number(days) || 7,
    })
  }

  return (
    <form onSubmit={handleSubmit} aria-label="Critères de repas">
      <label>
        Saison
        <select value={season} onChange={(event) => setSeason(event.target.value as SeasonName | '')}>
          <option value="">Peu importe</option>
          {seasons.map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </select>
      </label>

      <label>
        Style
        <select value={style} onChange={(event) => setStyle(event.target.value as MealStyleName | '')}>
          <option value="">Peu importe</option>
          {mealStyles.map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </select>
      </label>

      <label>
        Nombre de jours
        <input
          type="number"
          min={1}
          max={30}
          value={days}
          onChange={(event) => setDays(event.target.value)}
        />
      </label>

      <label>
        Ingrédients disponibles
        <input
          type="text"
          value={draft}
          placeholder="ex. jambon"
          onChange={(event) => setDraft(event.target.value)}
          onKeyDown={handleIngredientKeyDown}
        />
      </label>
      <button type="button" onClick={addIngredient}>
        Ajouter l'ingrédient
      </button>

      {ingredients.length > 0 && (
        <ul aria-label="Ingrédients ajoutés">
          {ingredients.map((ingredient) => (
            <li key={ingredient}>
              {ingredient}
              <button
                type="button"
                aria-label={`Retirer ${ingredient}`}
                onClick={() => removeIngredient(ingredient)}
              >
                ×
              </button>
            </li>
          ))}
        </ul>
      )}

      <button type="submit">Générer des idées</button>
    </form>
  )
}
