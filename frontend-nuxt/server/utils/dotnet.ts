import type { FetchError, FetchOptions } from 'ofetch'
import type { H3Event } from 'h3'

/** Per-status messages a handler wants instead of the defaults below. */
type StatusMessages = Record<number, string>

type ApiOptions = Omit<FetchOptions<'json'>, 'baseURL'>

const DEFAULT_MESSAGES: StatusMessages = {
  401: 'Not authenticated',
  403: 'Not authorized',
  404: 'Not found'
}

/**
 * Rebuilt on every attempt rather than computed once: after a refresh the token
 * has changed, and the retry must carry the new one.
 */
function buildOptions(event: H3Event, options: ApiOptions) {
  const token = getAuthToken(event)

  return {
    ...options,
    baseURL: useRuntimeConfig(event).apiBase,
    headers: {
      // Omitted entirely when absent, so .NET answers a clean 401 rather than
      // rejecting a malformed header.
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers
    }
  }
}

/**
 * The single door to .NET. Resolves the base URL from runtime config and
 * translates the browser's HttpOnly cookie into the `Authorization: Bearer`
 * header .NET expects — the credential swap that is the whole point of the BFF.
 *
 * A 401 is retried once behind a refresh, so an access token expiring mid-visit
 * is invisible to the caller. Throws the raw FetchError; pass it to `apiError`.
 */
export async function callApi<T>(
  event: H3Event,
  path: string,
  options: ApiOptions = {}
): Promise<T> {
  try {
    return await $fetch<T>(path, buildOptions(event, options))
  } catch (err) {
    const status = (err as FetchError).status

    // Only an expired access token is worth retrying, and only once: a second
    // 401 means the refresh itself is not being honoured.
    if (status !== 401 || !getRefreshToken(event)) throw err
    if (!(await refreshSession(event))) throw err

    return await $fetch<T>(path, buildOptions(event, options))
  }
}

/**
 * Same request, full response. Needed only where a `Set-Cookie` header matters,
 * since `$fetch` resolves to the parsed body and discards everything else.
 * No refresh-and-retry: the endpoints that need this have no session yet.
 */
export async function callApiRaw<T>(
  event: H3Event,
  path: string,
  options: ApiOptions = {}
) {
  return await $fetch.raw<T>(path, buildOptions(event, options))
}

/**
 * Normalises an upstream failure into an H3Error. Two cases that look alike and
 * are not: a response we did not like (has `.status`, pass it through) and no
 * response at all (connection refused, DNS, TLS, timeout — only 502 is honest).
 */
export function apiError(err: unknown, messages: StatusMessages = {}) {
  const e = err as FetchError

  if (e.status) {
    return createError({
      statusCode: e.status,
      statusMessage:
        messages[e.status]
        ?? DEFAULT_MESSAGES[e.status]
        ?? 'Upstream API rejected the request'
    })
  }

  // `data`, never `statusMessage`: that becomes the HTTP reason phrase, and a
  // newline or non-ASCII byte in it throws ERR_INVALID_CHAR.
  return createError({
    statusCode: 502,
    statusMessage: 'No response from the reporting API',
    data: import.meta.dev ? { cause: e.message } : undefined
  })
}
