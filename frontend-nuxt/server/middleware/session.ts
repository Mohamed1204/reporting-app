/** Refresh this many seconds before the token actually expires. */
const SKEW_SECONDS = 30

/**
 * Renews an expiring access token before anything tries to use it.
 *
 * Done here, proactively, rather than reacting to a 401 further in — because
 * "further in" is often an in-process API call during SSR, whose response (and
 * therefore whose Set-Cookie) is discarded. This runs on the real request, so
 * the browser actually receives the rotated cookies.
 *
 * `exp` is read locally, so the common case costs no round trip at all.
 */
export default defineEventHandler(async (event) => {
  if (!getRefreshToken(event)) return

  const token = getAuthToken(event)
  if (token && !isExpiring(token)) return

  await refreshSession(event)
  syncRequestCookies(event)
})

function isExpiring(token: string): boolean {
  const exp = readJwtExpiry(token)

  // Unreadable expiry: let .NET be the judge rather than refreshing on a guess.
  if (!exp) return false

  return exp - SKEW_SECONDS <= Math.floor(Date.now() / 1000)
}
