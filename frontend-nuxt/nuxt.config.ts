// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  app: {
    head: {
      htmlAttrs: { lang: 'en' }
    }
  },

  runtimeConfig: {
    apiBase: 'http://localhost:5247',
    public: {
      apiBase: 'https://localhost:7033'
    }
  },

  routeRules: {
    // Static form, identical for every visitor, no data of its own — so the
    // HTML can exist before anyone asks for it. Note the cost: with no server
    // run at request time, the "already logged in" bounce can only happen
    // client-side, after hydration.
    '/auth': { prerender: true },

    // Every response here is scoped to one user's cookie. `no-store` keeps a
    // CDN or corporate proxy from serving one filer's periods to another.
    '/api/**': { headers: { 'cache-control': 'no-store' } }
  }

  // Deliberately absent: `ssr: false` on the dashboard, and `swr`/`isr` caching.
  // The first would undo Phase 4 — the reason the BFF holds a token at all is
  // so authenticated pages can render on the server. The second cannot apply to
  // a response that differs per user; a shared cache is exactly what `no-store`
  // above is preventing.
})
