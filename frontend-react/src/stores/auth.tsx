import { createContext, useContext, useState, type ReactNode } from 'react'

// Port of frontend/src/stores/auth.ts.
// Step 2: implement this with Context + useState first, then try rewriting it with Zustand.

export interface Session {
  token: string
  role: string
  userName: string
  companyName: string
}

export interface AuthStore {
  session: Session | null
  isLoggedIn: boolean
  login: (userName: string, password: string) => Promise<boolean>
  refresh: () => Promise<boolean>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthStore | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session] = useState<Session | null>(null)

  // TODO: POST /api/Auth/login with { UserName, Password }, credentials: 'include'
  async function login(_userName: string, _password: string): Promise<boolean> {
    throw new Error('login: not implemented')
  }

  // TODO: POST /api/Auth/refresh (uses the HttpOnly refresh cookie)
  async function refresh(): Promise<boolean> {
    throw new Error('refresh: not implemented')
  }

  // TODO: POST /api/Auth/logout (best effort), then clear the session
  async function logout(): Promise<void> {
    throw new Error('logout: not implemented')
  }

  const value: AuthStore = {
    session,
    isLoggedIn: session !== null,
    login,
    refresh,
    logout,
  }

  return <AuthContext value={value}>{children}</AuthContext>
}

export function useAuth(): AuthStore {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>')
  return ctx
}
