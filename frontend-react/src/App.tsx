import { Outlet } from 'react-router'

// Port of frontend/src/App.vue. Outlet is <RouterView />.
// TODO: the nav (Open Periods link, company/role/user info, Logout button) using useAuth().
export default function App() {
  return (
    <div className="app-shell">
      <nav>
        <span className="app-version">v.1 (react)</span>
      </nav>
      <Outlet />
    </div>
  )
}
