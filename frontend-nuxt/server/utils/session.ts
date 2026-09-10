import type { H3Event } from 'h3'

export const TOKEN_COOKIE = 'token'
export const SESSION_COOKIE = 'session'
export const REFRESH_COOKIE = 'refreshToken'

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

  // See getAuthToken: setCookie writes to the *response*, so a later read in
  // this same request would otherwise still see the pre-refresh token.
  event.context.authToken = auth.token

  return session
}

/**
 * Nitro's own copy of .NET's refresh token. .NET set it on its own origin, where
 * the browser will never see it — this re-issues it on ours.
 */
export function setRefreshCookie(event: H3Event, token: string, expires?: Date) {
  setCookie(event, REFRESH_COOKIE, token, {
    ...baseOptions(),
    httpOnly: true,
    expires
  })

  event.context.refreshToken = token
}

/**
 * Reads the context before the cookie. Cookies set during this request live on
 * the *response*; `getCookie` only ever sees what the browser sent. Without the
 * context fallback, a retry after a refresh would resend the expired token —
 * and for the refresh token that means replaying a rotated one, which .NET
 * treats as theft and answers by revoking the whole family.
 */
export function getAuthToken(event: H3Event): string | null {
  return event.context.authToken ?? getCookie(event, TOKEN_COOKIE) ?? null
}

export function getRefreshToken(event: H3Event): string | null {
  return event.context.refreshToken ?? getCookie(event, REFRESH_COOKIE) ?? null
}

/**
 * Rewrites the *incoming* cookie header to match what we just set.
 *
 * During SSR, Nitro copies the page request's headers into its in-process API
 * calls — that is how 4.3's forwarding works. Those copies are taken when the
 * fetch happens, which is after middleware has run, so rewriting here means the
 * sub-requests carry the fresh token.
 *
 * It matters because a sub-request gets its own event with its own response,
 * and h3 does not share `context` into it (verified: the page and its
 * `/api/periods` call report different context objects). Anything it sets via
 * Set-Cookie is written to a response that is discarded once the body is read —
 * so the refresh has to happen out here, on the real request, instead.
 */
export function syncRequestCookies(event: H3Event) {
  const jar = parseCookies(event)

  for (const [name, value] of [
    [TOKEN_COOKIE, event.context.authToken],
    [REFRESH_COOKIE, event.context.refreshToken]
  ] as const) {
    if (value) jar[name] = value
    else delete jar[name]
  }

  const header = Object.entries(jar)
    .map(([name, value]) => `${name}=${encodeURIComponent(value)}`)
    .join('; ')

  event.node.req.headers.cookie = header || undefined
}

export function clearAuthCookies(event: H3Event) {
  deleteCookie(event, TOKEN_COOKIE, { ...baseOptions(), httpOnly: true })
  deleteCookie(event, SESSION_COOKIE, { ...baseOptions(), httpOnly: false })
  deleteCookie(event, REFRESH_COOKIE, { ...baseOptions(), httpOnly: true })

  event.context.authToken = null
  event.context.refreshToken = null
}
