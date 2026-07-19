import { useEffect, useState } from 'react'

import {
  createRecipe,
  deleteRecipe,
  listRecipes,
  updateRecipe,
} from '../../api/recipesClient'
import type { Recipe, RecipeInput } from '../../api/types'
import { RecipeForm } from '../../components/RecipeForm/RecipeForm'

type Mode = { kind: 'list' } | { kind: 'create' } | { kind: 'edit'; recipe: Recipe }

export function RecipesPage() {
  const [recipes, setRecipes] = useState<Recipe[]>([])
  const [status, setStatus] = useState<'loading' | 'error' | 'idle'>('loading')
  const [mode, setMode] = useState<Mode>({ kind: 'list' })
  const [pendingDelete, setPendingDelete] = useState<string | null>(null)

  const refresh = () => {
    setStatus('loading')
    listRecipes()
      .then((loaded) => {
        setRecipes(loaded)
        setStatus('idle')
      })
      .catch(() => setStatus('error'))
  }

  useEffect(refresh, [])

  const handleCreate = async (input: RecipeInput) => {
    await createRecipe(input)
    setMode({ kind: 'list' })
    refresh()
  }

  const handleUpdate = (id: string) => async (input: RecipeInput) => {
    await updateRecipe(id, input)
    setMode({ kind: 'list' })
    refresh()
  }

  const handleDelete = async (id: string) => {
    await deleteRecipe(id)
    setPendingDelete(null)
    refresh()
  }

  if (mode.kind === 'create') {
    return (
      <>
        <h1>Nouvelle recette</h1>
        <RecipeForm
          submitLabel="Créer la recette"
          onSubmit={handleCreate}
          onCancel={() => setMode({ kind: 'list' })}
        />
      </>
    )
  }

  if (mode.kind === 'edit') {
    const { id, ...rest } = mode.recipe
    return (
      <>
        <h1>Modifier la recette</h1>
        <RecipeForm
          initialValue={rest}
          submitLabel="Enregistrer"
          onSubmit={handleUpdate(id)}
          onCancel={() => setMode({ kind: 'list' })}
        />
      </>
    )
  }

  return (
    <>
      <div className="recipes__header">
        <h1>Mes recettes</h1>
        <button type="button" onClick={() => setMode({ kind: 'create' })}>
          Ajouter une recette
        </button>
      </div>

      {status === 'loading' && <p role="status">Chargement…</p>}
      {status === 'error' && (
        <p role="alert" className="app__error">
          Impossible de charger les recettes. Vérifie que l'API et la base de données sont démarrées.
        </p>
      )}

      {status === 'idle' && recipes.length === 0 && (
        <p role="status">Aucune recette pour l'instant. Ajoute la première !</p>
      )}

      <ul className="app__results">
        {recipes.map((recipe) => (
          <li key={recipe.id} className="app__card">
            <h2>{recipe.name}</h2>
            {recipe.styles.length > 0 && (
              <ul className="app__styles" aria-label="Styles">
                {recipe.styles.map((style) => (
                  <li key={style} className="app__style">
                    {style}
                  </li>
                ))}
              </ul>
            )}
            {recipe.description && <p>{recipe.description}</p>}
            <p className="app__meta">⏱ {recipe.prepTimeMinutes} min</p>
            {recipe.seasons.length > 0 && (
              <p className="app__meta">📅 {recipe.seasons.join(', ')}</p>
            )}
            {recipe.ingredients.length > 0 && (
              <p className="app__meta">🧺 {recipe.ingredients.join(', ')}</p>
            )}

            <div className="recipe-card__actions">
              <button type="button" onClick={() => setMode({ kind: 'edit', recipe })}>
                Modifier
              </button>
              {pendingDelete === recipe.id ? (
                <>
                  <span className="recipe-card__confirm">Confirmer la suppression ?</span>
                  <button type="button" onClick={() => void handleDelete(recipe.id)}>
                    Oui, supprimer
                  </button>
                  <button type="button" onClick={() => setPendingDelete(null)}>
                    Annuler
                  </button>
                </>
              ) : (
                <button
                  type="button"
                  className="recipe-card__delete"
                  onClick={() => setPendingDelete(recipe.id)}
                >
                  Supprimer
                </button>
              )}
            </div>
          </li>
        ))}
      </ul>
    </>
  )
}
