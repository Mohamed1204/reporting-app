import { Outlet } from 'react-router'

// Step 3: React Router has no global beforeEach. This layout route plays that role.
// TODO: port the guard from frontend/src/router/index.ts:
//   - not logged in            -> <Navigate to="/auth" replace />
//   - /periods/review without a selected period -> back to "/"
//   - (and /auth while logged in -> "/", which needs to live somewhere else)
export function RequireAuth() {
  return <Outlet />
}
