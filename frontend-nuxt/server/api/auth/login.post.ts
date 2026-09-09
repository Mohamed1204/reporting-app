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
    // No cookie exists yet, so callApi sends no Authorization header — which is
    // what /login wants.
    const auth = await callApi<AuthResponse>(event, '/api/Auth/login', {
      method: 'POST',
      body: {
        UserName: body.UserName,
        Password: body.Password
      }
    })

    // The token stops here. Only the display fields continue to the browser.
    return setAuthCookies(event, auth)
  } catch (err) {
    throw apiError(err, { 401: 'Invalid username or password' })
  }
})
