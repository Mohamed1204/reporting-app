import type { FetchError } from 'ofetch'

interface LoginBody {
  UserName?: string
  Password?: string
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)

  const body = await readBody<LoginBody>(event)

  if (!body?.UserName || !body?.Password) {
    throw createError({ statusCode: 400, statusMessage: 'Username and password are required' })
  }

  try {
    const auth = await $fetch<AuthResponse>(`${config.apiBase}/api/Auth/login`, {
      method: 'POST',
      body: {
        UserName: body.UserName,
        Password: body.Password
      }
    })

    // The token stops here. Only the display fields continue to the browser.
    return setAuthCookies(event, auth)
  } catch (err) {
    const e = err as FetchError

    if (e.status) {
      throw createError({
        statusCode: e.status,
        statusMessage: e.status === 401
          ? 'Invalid username or password'
          : 'Upstream API rejected the request'
      })
    }

    // No HTTP response at all: connection refused, DNS, TLS, or timeout.
    throw createError({
      statusCode: 502,
      statusMessage: 'No response from the reporting API',
      data: import.meta.dev ? { cause: e.message } : undefined
    })
  }
})
