import type { H3Event } from 'h3'

export const TOKEN_COOKIE = 'token'
export const SESSION_COOKIE = 'session'

/** Fallback lifetime if the token carries no readable `exp`. */
const FALLBACK_MAX_AGE = 15 * 60

/**
 * Shared by every set/delete below. A cookie is only overwritten or removed when
 * these attributes match the ones it was set with — keeping them in one place is
 * what stops "logout doesn't log out".
 */
function baseOptions() {
  return {
    path: '/',
    // Not 'strict': that withholds the cookie on top-level navigation from
    // another site, so an SSR render of an inbound link would look logged out.
    sameSite: 'lax',
    secure: !import.meta.dev
  } as const
}

export function setAuthCookies(event: H3Event, auth: AuthResponse): SessionUser {
  const exp = readJwtExpiry(auth.token)
  const maxAge = exp
    ? Math.max(0, exp - Math.floor(Date.now() / 1000))
    : FALLBACK_MAX_AGE

  const session: SessionUser = {
    role: auth.role,
    userName: auth.userName,
    companyName: auth.companyName
  }

  // The credential. Unreadable from JavaScript.
  setCookie(event, TOKEN_COOKIE, auth.token, {
    ...baseOptions(),
    httpOnly: true,
    maxAge
  })

  // Display data only — never trust it for authorization.
  setCookie(event, SESSION_COOKIE, JSON.stringify(session), {
    ...baseOptions(),
    httpOnly: false,
    maxAge
  })

  return session
}

export function getAuthToken(event: H3Event): string | null {
  return getCookie(event, TOKEN_COOKIE) ?? null
}

export function clearAuthCookies(event: H3Event) {
  deleteCookie(event, TOKEN_COOKIE, { ...baseOptions(), httpOnly: true })
  deleteCookie(event, SESSION_COOKIE, { ...baseOptions(), httpOnly: false })
}
