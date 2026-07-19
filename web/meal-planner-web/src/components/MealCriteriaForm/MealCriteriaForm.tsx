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

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    onSubmit({
      season: season === '' ? null : season,
      style: style === '' ? null : style,
      maxPrepTimeMinutes: null,
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

      <button type="submit">Générer des idées</button>
    </form>
  )
}
