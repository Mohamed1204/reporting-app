import './assets/main.css'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router'

import { router } from './router'
import { AuthProvider } from './stores/auth'

// TODO (step 2): the Vue app calls authStore.refresh() *before* mounting so a page reload
// keeps you logged in (frontend/src/main.ts). Decide where that belongs here:
// before createRoot, or inside AuthProvider with a "checking session" state.
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  </StrictMode>,
)
