import type { FetchError, FetchOptions } from 'ofetch'
import type { H3Event } from 'h3'

/** Per-status messages a handler wants instead of the defaults below. */
type StatusMessages = Record<number, string>

const DEFAULT_MESSAGES: StatusMessages = {
  401: 'Not authenticated',
  403: 'Not authorized',
  404: 'Not found'
}

/**
 * The single door to .NET. Resolves the base URL from runtime config and
 * translates the browser's HttpOnly cookie into the `Authorization: Bearer`
 * header .NET expects — the credential swap that is the whole point of the BFF.
 *
 * Throws the raw FetchError; pass it to `apiError` in the handler's catch.
 */
export async function callApi<T>(
  event: H3Event,
  path: string,
  options: Omit<FetchOptions<'json'>, 'baseURL'> = {}
): Promise<T> {
  const config = useRuntimeConfig(event)
  const token = getAuthToken(event)

  return await $fetch<T>(path, {
    ...options,
    baseURL: config.apiBase,
    headers: {
      // Omitted entirely when absent, so .NET answers a clean 401 rather than
      // rejecting a malformed header.
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers
    }
  })
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
