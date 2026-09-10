interface LoginBody {
  UserName?: string
  Password?: string
}

export default defineEventHandler(async (event) => {
  const body = await readBody<LoginBody>(event)

  if (!body?.UserName || !body?.Password) {
    throw createError({ statusCode: 400, statusMessage: 'Username and password are required' })
  }

  try {
    // Raw, because .NET returns the refresh token as a `Set-Cookie` header and
    // plain `$fetch` would hand back only the body.
    const res = await callApiRaw<AuthResponse>(event, '/api/Auth/login', {
      method: 'POST',
      body: {
        UserName: body.UserName,
        Password: body.Password
      }
    })

    if (!res._data) {
      throw createError({ statusCode: 502, statusMessage: 'Malformed response from the reporting API' })
    }

    // That cookie was set on .NET's origin, where the browser will never send
    // it back. Re-issue it on ours.
    captureRefreshCookie(event, res)

    // The token stops here. Only the display fields continue to the browser.
    return setAuthCookies(event, res._data)
  } catch (err) {
    if (isError(err)) throw err

    throw apiError(err, { 401: 'Invalid username or password' })
  }
})
