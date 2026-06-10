import { defineConfig } from 'cypress'

export default defineConfig({
  e2e: {
    // The app is served by `vite preview`, which defaults to port 4173.
    baseUrl: 'http://localhost:4173',
    specPattern: 'cypress/e2e/**/*.cy.{js,ts}',
    // No custom commands/global setup yet, so skip the support file.
    supportFile: false,
  },
})
