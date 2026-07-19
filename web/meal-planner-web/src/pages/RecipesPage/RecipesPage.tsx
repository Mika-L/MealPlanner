import { useEffect, useMemo, useState } from 'react'

import {
  createRecipe,
  deleteRecipe,
  listRecipes,
  updateRecipe,
} from '../../api/recipesClient'
import type { Recipe, RecipeInput } from '../../api/types'
import { RecipeForm } from '../../components/RecipeForm/RecipeForm'

type Mode = { kind: 'list' } | { kind: 'create' } | { kind: 'edit'; recipe: Recipe }

// Nombre de cartes rendues par palier : borne le DOM même avec des centaines de recettes.
const RECIPES_PER_PAGE = 24

export function RecipesPage() {
  const [recipes, setRecipes] = useState<Recipe[]>([])
  const [status, setStatus] = useState<'loading' | 'error' | 'idle'>('loading')
  const [mode, setMode] = useState<Mode>({ kind: 'list' })
  const [pendingDelete, setPendingDelete] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [visibleCount, setVisibleCount] = useState(RECIPES_PER_PAGE)

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

  // Découple la frappe du filtrage : on ne balaie les recettes qu'après une courte pause.
  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedSearch(search), 150)
    return () => clearTimeout(timeout)
  }, [search])

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

  // Index de recherche pré-calculé une fois par liste : évite de reconstruire les chaînes à chaque frappe.
  const searchIndex = useMemo(
    () =>
      recipes.map((recipe) => ({
        recipe,
        haystack: [
          recipe.name,
          recipe.description,
          ...recipe.ingredients,
          ...recipe.styles,
          ...recipe.seasons,
        ]
          .join(' ')
          .toLowerCase(),
      })),
    [recipes],
  )

  const query = debouncedSearch.trim().toLowerCase()
  const filteredRecipes = useMemo(
    () =>
      query
        ? searchIndex.filter((entry) => entry.haystack.includes(query)).map((entry) => entry.recipe)
        : recipes,
    [searchIndex, recipes, query],
  )

  // Repart du premier palier quand la recherche change.
  useEffect(() => {
    setVisibleCount(RECIPES_PER_PAGE)
  }, [query])

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

  const visibleRecipes = filteredRecipes.slice(0, visibleCount)
  const remaining = filteredRecipes.length - visibleRecipes.length

  return (
    <>
      <div className="recipes__header">
        <h1>Mes recettes</h1>
        <button type="button" onClick={() => setMode({ kind: 'create' })}>
          Ajouter une recette
        </button>
      </div>

      {status === 'idle' && recipes.length > 0 && (
        <div className="recipes__search">
          <input
            type="text"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Rechercher une recette, un ingrédient, un style…"
            aria-label="Rechercher une recette"
          />
          {filteredRecipes.length > 0 && (
            <p className="recipes__count" role="status">
              {query
                ? `${filteredRecipes.length} résultat${filteredRecipes.length > 1 ? 's' : ''} sur ${recipes.length}`
                : `${recipes.length} recette${recipes.length > 1 ? 's' : ''}`}
            </p>
          )}
        </div>
      )}

      {status === 'loading' && <p role="status">Chargement…</p>}
      {status === 'error' && (
        <p role="alert" className="app__error">
          Impossible de charger les recettes. Vérifie que l'API et la base de données sont démarrées.
        </p>
      )}

      {status === 'idle' && recipes.length === 0 && (
        <p role="status">Aucune recette pour l'instant. Ajoute la première !</p>
      )}

      {status === 'idle' && recipes.length > 0 && filteredRecipes.length === 0 && (
        <p role="status">Aucune recette ne correspond à « {query} ».</p>
      )}

      <ul className="app__results">
        {visibleRecipes.map((recipe) => (
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

      {remaining > 0 && (
        <div className="recipes__more">
          <button
            type="button"
            onClick={() => setVisibleCount((count) => count + RECIPES_PER_PAGE)}
          >
            Afficher plus ({remaining} restante{remaining > 1 ? 's' : ''})
          </button>
        </div>
      )}
    </>
  )
}
