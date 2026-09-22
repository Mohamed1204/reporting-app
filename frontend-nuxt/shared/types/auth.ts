/** What .NET's POST /api/Auth/login returns. */
export interface AuthResponse {
  token: string
  role: string
  userName: string
  companyName: string
}

/** What our BFF returns to the browser — the same minus the credential. */
export type SessionUser = Omit<AuthResponse, 'token'>
