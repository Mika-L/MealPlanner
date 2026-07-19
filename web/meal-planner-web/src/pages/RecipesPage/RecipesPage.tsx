import { useCallback, useEffect, useRef, useState } from 'react'

import {
  createRecipe,
  deleteRecipe,
  listRecipes,
  updateRecipe,
} from '../../api/recipesClient'
import type { Recipe, RecipeInput } from '../../api/types'
import { RecipeForm } from '../../components/RecipeForm/RecipeForm'

type Mode = { kind: 'list' } | { kind: 'create' } | { kind: 'edit'; recipe: Recipe }
type Status = 'loading' | 'loadingMore' | 'error' | 'idle'

// Taille de page demandée à l'API : borne le volume transféré et rendu, même avec des centaines de recettes.
const PAGE_SIZE = 24

export function RecipesPage() {
  const [recipes, setRecipes] = useState<Recipe[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<Status>('loading')
  const [mode, setMode] = useState<Mode>({ kind: 'list' })
  const [pendingDelete, setPendingDelete] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')

  // Annule la requête précédente : une recherche rapide ou un « Afficher plus » ne doit jamais
  // laisser une réponse tardive écraser la plus récente.
  const requestRef = useRef<AbortController | null>(null)

  const load = useCallback((term: string, pageToLoad: number, append: boolean) => {
    requestRef.current?.abort()
    const controller = new AbortController()
    requestRef.current = controller
    setStatus(append ? 'loadingMore' : 'loading')

    listRecipes({ search: term, page: pageToLoad, pageSize: PAGE_SIZE }, controller.signal)
      .then((result) => {
        setRecipes((previous) => (append ? [...previous, ...result.recipes] : result.recipes))
        setTotal(result.total)
        setPage(result.page)
        setStatus('idle')
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setStatus('error')
        }
      })
  }, [])

  // Découple la frappe des requêtes : on n'interroge l'API qu'après une courte pause.
  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedSearch(search), 250)
    return () => clearTimeout(timeout)
  }, [search])

  // Toute nouvelle recherche (et le premier rendu) repart de la page 1.
  useEffect(() => {
    load(debouncedSearch, 1, false)
  }, [debouncedSearch, load])

  const handleCreate = async (input: RecipeInput) => {
    await createRecipe(input)
    setMode({ kind: 'list' })
    load(debouncedSearch, 1, false)
  }

  const handleUpdate = (id: string) => async (input: RecipeInput) => {
    await updateRecipe(id, input)
    setMode({ kind: 'list' })
    load(debouncedSearch, 1, false)
  }

  const handleDelete = async (id: string) => {
    await deleteRecipe(id)
    setPendingDelete(null)
    load(debouncedSearch, 1, false)
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

  const activeSearch = debouncedSearch.trim()
  const showSearch = total > 0 || activeSearch !== ''
  const remaining = total - recipes.length

  return (
    <>
      <div className="recipes__header">
        <h1>Mes recettes</h1>
        <button type="button" onClick={() => setMode({ kind: 'create' })}>
          Ajouter une recette
        </button>
      </div>

      {showSearch && (
        <div className="recipes__search">
          <input
            type="text"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Rechercher une recette, un ingrédient, un style…"
            aria-label="Rechercher une recette"
          />
          {total > 0 && (
            <p className="recipes__count" role="status">
              {activeSearch
                ? `${total} résultat${total > 1 ? 's' : ''}`
                : `${total} recette${total > 1 ? 's' : ''}`}
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

      {status === 'idle' && total === 0 && activeSearch === '' && (
        <p role="status">Aucune recette pour l'instant. Ajoute la première !</p>
      )}

      {status === 'idle' && total === 0 && activeSearch !== '' && (
        <p role="status">Aucune recette ne correspond à « {activeSearch} ».</p>
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

      {remaining > 0 && (
        <div className="recipes__more">
          <button
            type="button"
            disabled={status === 'loadingMore'}
            onClick={() => load(debouncedSearch, page + 1, true)}
          >
            {status === 'loadingMore'
              ? 'Chargement…'
              : `Afficher plus (${remaining} restante${remaining > 1 ? 's' : ''})`}
          </button>
        </div>
      )}
    </>
  )
}
