/** Opt a page out of the global auth middleware: definePageMeta({ public: true }) */
declare module '#app' {
  interface PageMeta {
    public?: boolean
  }
}

declare module 'vue-router' {
  interface RouteMeta {
    public?: boolean
  }
}

export {}
