export default defineEventHandler(async (event) => {
  const refreshToken = getRefreshToken(event)

  if (refreshToken) {
    try {
      // Revokes the token server-side, so it stays dead even if a copy leaked.
      // Raw, to skip the refresh-and-retry in callApi — a 401 here just means
      // the token was already invalid, which is the outcome we wanted anyway.
      await callApiRaw(event, '/api/Auth/logout', {
        method: 'POST',
        headers: { cookie: `${REFRESH_COOKIE}=${refreshToken}` }
      })
    } catch {
      // .NET being unreachable must not strand the user logged in locally.
      // Clearing our own cookies below is the part that has to happen.
    }

    await forgetRefresh(refreshToken)
  }

  clearAuthCookies(event)
  setResponseStatus(event, 204)
  return null
})
