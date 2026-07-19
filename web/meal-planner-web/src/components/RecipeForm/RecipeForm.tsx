import { useState } from 'react'

import {
  mealStyles,
  seasons,
  type MealStyleName,
  type RecipeInput,
  type SeasonName,
} from '../../api/types'

interface RecipeFormProps {
  initialValue?: RecipeInput
  submitLabel: string
  onSubmit: (input: RecipeInput) => void
  onCancel: () => void
}

const emptyRecipe: RecipeInput = {
  name: '',
  description: '',
  seasons: [],
  styles: [],
  prepTimeMinutes: 30,
  ingredients: [],
}

// Ajoute/retire une valeur d'un tableau de flags (saison ou style), sans doublon.
function toggle<T>(values: T[], value: T): T[] {
  return values.includes(value) ? values.filter((item) => item !== value) : [...values, value]
}

export function RecipeForm({ initialValue, submitLabel, onSubmit, onCancel }: RecipeFormProps) {
  const [name, setName] = useState(initialValue?.name ?? emptyRecipe.name)
  const [description, setDescription] = useState(initialValue?.description ?? emptyRecipe.description)
  const [selectedSeasons, setSelectedSeasons] = useState<SeasonName[]>(
    initialValue?.seasons ?? emptyRecipe.seasons,
  )
  const [selectedStyles, setSelectedStyles] = useState<MealStyleName[]>(
    initialValue?.styles ?? emptyRecipe.styles,
  )
  const [prepTime, setPrepTime] = useState(
    String(initialValue?.prepTimeMinutes ?? emptyRecipe.prepTimeMinutes),
  )
  const [ingredients, setIngredients] = useState<string[]>(
    initialValue?.ingredients ?? emptyRecipe.ingredients,
  )
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
      name: name.trim(),
      description: description.trim(),
      seasons: selectedSeasons,
      styles: selectedStyles,
      prepTimeMinutes: Number(prepTime) || 0,
      ingredients,
    })
  }

  return (
    <form className="recipe-form" onSubmit={handleSubmit} aria-label="Recette">
      <label>
        Nom
        <input
          type="text"
          value={name}
          required
          maxLength={200}
          onChange={(event) => setName(event.target.value)}
        />
      </label>

      <label>
        Description
        <textarea
          value={description}
          rows={3}
          maxLength={2000}
          onChange={(event) => setDescription(event.target.value)}
        />
      </label>

      <fieldset className="recipe-form__group">
        <legend>Saisons</legend>
        {seasons.map((value) => (
          <label key={value} className="recipe-form__check">
            <input
              type="checkbox"
              checked={selectedSeasons.includes(value)}
              onChange={() => setSelectedSeasons((current) => toggle(current, value))}
            />
            {value}
          </label>
        ))}
      </fieldset>

      <fieldset className="recipe-form__group">
        <legend>Styles</legend>
        {mealStyles.map((value) => (
          <label key={value} className="recipe-form__check">
            <input
              type="checkbox"
              checked={selectedStyles.includes(value)}
              onChange={() => setSelectedStyles((current) => toggle(current, value))}
            />
            {value}
          </label>
        ))}
      </fieldset>

      <label>
        Temps de préparation (min)
        <input
          type="number"
          min={1}
          value={prepTime}
          required
          onChange={(event) => setPrepTime(event.target.value)}
        />
      </label>

      <label>
        Ingrédients
        <input
          type="text"
          value={draft}
          placeholder="ex. tomate"
          onChange={(event) => setDraft(event.target.value)}
          onKeyDown={handleIngredientKeyDown}
        />
      </label>
      <button type="button" onClick={addIngredient}>
        Ajouter l'ingrédient
      </button>

      {ingredients.length > 0 && (
        <ul className="recipe-form__ingredients" aria-label="Ingrédients ajoutés">
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

      <div className="recipe-form__actions">
        <button type="submit">{submitLabel}</button>
        <button type="button" onClick={onCancel}>
          Annuler
        </button>
      </div>
    </form>
  )
}
