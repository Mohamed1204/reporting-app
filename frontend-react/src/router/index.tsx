import { createBrowserRouter } from 'react-router'

import App from '../App'
import HomeView from '../views/HomeView'
import { RequireAuth } from './RequireAuth'

// Port of frontend/src/router/index.ts. Same paths as the Vue SPA.
export const router = createBrowserRouter([
  {
    element: <App />,
    children: [
      {
        path: '/auth',
        lazy: async () => ({ Component: (await import('../views/LoginRegisterView')).default }),
      },
      {
        element: <RequireAuth />,
        children: [
          { path: '/', element: <HomeView /> },
          {
            path: '/periods/review',
            lazy: async () => ({ Component: (await import('../views/PeriodReviewView')).default }),
          },
        ],
      },
    ],
  },
])
