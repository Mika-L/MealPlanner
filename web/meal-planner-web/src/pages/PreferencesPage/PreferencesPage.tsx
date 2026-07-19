import { themes, useTheme } from '../../theme'

export function PreferencesPage() {
  const { theme, setTheme } = useTheme()

  return (
    <>
      <h1>Préférences</h1>
      <p className="app__lead">Personnalise l'apparence de MealPlanner.</p>

      <section className="prefs" aria-labelledby="prefs-theme-title">
        <h2 id="prefs-theme-title">Thème</h2>
        <p className="prefs__hint">
          Choisis la palette de couleurs. Le mode clair ou sombre, lui, suit automatiquement ton
          système.
        </p>

        <fieldset className="prefs__themes">
          <legend className="prefs__legend">Palette de couleurs</legend>
          {themes.map((option) => (
            <label
              key={option.id}
              className={`prefs__theme${theme === option.id ? ' prefs__theme--active' : ''}`}
            >
              <input
                type="radio"
                name="theme"
                value={option.id}
                checked={theme === option.id}
                onChange={() => setTheme(option.id)}
              />
              <span className="prefs__swatches" aria-hidden="true">
                {option.swatches.map((color, index) => (
                  <span
                    key={color + index}
                    className="prefs__swatch"
                    style={{ background: color }}
                  />
                ))}
              </span>
              <span className="prefs__theme-body">
                <span className="prefs__theme-name">{option.label}</span>
                <span className="prefs__theme-desc">{option.description}</span>
              </span>
            </label>
          ))}
        </fieldset>
      </section>
    </>
  )
}
