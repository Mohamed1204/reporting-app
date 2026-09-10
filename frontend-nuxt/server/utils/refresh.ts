import type { H3Event } from 'h3'
import type { FetchResponse } from 'ofetch'
import { parseSetCookie } from 'cookie-es'

interface RotatedSession {
  auth: AuthResponse
  refreshToken: string
  refreshExpires?: Date
}

/**
 * Refreshes keyed by the token being spent — both in-flight and recently
 * settled.
 *
 * .NET rotates refresh tokens and treats a replayed one as theft, revoking the
 * whole family (AuthService.cs:100). So a token may only ever be spent once,
 * and every request still holding it has to be served from that one result.
 *
 * De-duplicating only *concurrent* requests is not enough, and this is the part
 * that bites: a request arriving a moment after the first refresh settles is
 * still carrying the old cookie — its copy was captured before the rotation.
 * Spending it again is precisely the replay .NET punishes. So entries linger
 * for a grace window after resolving rather than being dropped immediately.
 *
 * Keyed rather than one module-level promise, because Nitro serves every user
 * from a single process: a lone shared promise would hand one user's refresh to
 * another. (The SPA gets away with that — one browser is one user.)
 *
 * Per-process, so several Nitro instances behind a load balancer would each
 * keep their own map. That needs sticky sessions or a shared store.
 */
const inFlight = new Map<string, Promise<RotatedSession | null>>()

/** How long a spent token keeps returning its replacement. */
const GRACE_MS = 10_000

function forgetAfterGrace(token: string) {
  // unref: a pending timer must not keep the process alive on shutdown.
  setTimeout(() => inFlight.delete(token), GRACE_MS).unref?.()
}

/**
 * Spends the refresh token for a new access token and re-issues both cookies.
 * Returns false when the session is beyond saving, having cleared it.
 */
export async function refreshSession(event: H3Event): Promise<boolean> {
  const current = getRefreshToken(event)
  if (!current) return false

  let pending = inFlight.get(current)
  if (!pending) {
    pending = rotate(useRuntimeConfig(event).apiBase, current)
      .finally(() => forgetAfterGrace(current))
    inFlight.set(current, pending)
  }

  const rotated = await pending

  if (!rotated) {
    clearAuthCookies(event)
    return false
  }

  // Written per-event, not inside `rotate`: concurrent requests share the
  // network call, but each one needs Set-Cookie on its own response.
  setAuthCookies(event, rotated.auth)
  setRefreshCookie(event, rotated.refreshToken, rotated.refreshExpires)
  return true
}

/**
 * Drops `token` and any grace entry that hands it out. Without this, logging
 * out inside the grace window leaves a spent predecessor token still able to
 * fetch the now-revoked session back out of the cache.
 */
export async function forgetRefresh(token: string) {
  inFlight.delete(token)

  for (const [key, pending] of inFlight) {
    const settled = await pending.catch(() => null)
    if (settled?.refreshToken === token) inFlight.delete(key)
  }
}

async function rotate(apiBase: string, refreshToken: string): Promise<RotatedSession | null> {
  try {
    const res = await $fetch.raw<AuthResponse>('/api/Auth/refresh', {
      baseURL: apiBase,
      method: 'POST',
      // .NET reads Request.Cookies["refreshToken"]. Node has no cookie jar, so
      // the header is assembled by hand.
      headers: { cookie: `${REFRESH_COOKIE}=${refreshToken}` }
    })

    const next = readRefreshCookie(res)
    if (!res._data || !next) return null

    return { auth: res._data, ...next }
  } catch {
    // 401 means the refresh token is expired, revoked, or already spent. Either
    // way there is nothing left to try.
    return null
  }
}

/**
 * `$fetch` resolves to the parsed body and drops the headers, so .NET's rotated
 * cookie is only reachable from a `.raw()` response.
 */
export function readRefreshCookie(res: FetchResponse<unknown>) {
  for (const header of res.headers.getSetCookie()) {
    const parsed = parseSetCookie(header)
    if (parsed.name !== REFRESH_COOKIE || !parsed.value) continue

    return { refreshToken: parsed.value, refreshExpires: parsed.expires }
  }

  return null
}

/** Re-issues .NET's refresh cookie on our origin, if the response carried one. */
export function captureRefreshCookie(event: H3Event, res: FetchResponse<unknown>) {
  const next = readRefreshCookie(res)
  if (next) setRefreshCookie(event, next.refreshToken, next.refreshExpires)
}
