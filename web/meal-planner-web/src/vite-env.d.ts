/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** ID client OAuth Google (public) : audience des id_token émis côté navigateur. */
  readonly VITE_GOOGLE_CLIENT_ID?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
