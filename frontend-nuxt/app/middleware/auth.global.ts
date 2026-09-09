export default defineNuxtRouteMiddleware((to) => {
  // Runs on the server for the first request and in the browser thereafter,
  // so nothing here may touch window/document/localStorage.
  //
  // Reads `session`, never `token`: the token cookie is HttpOnly, so it is
  // invisible to useCookie in the browser and every client-side navigation
  // would look logged out. Safe to key on, because this only decides what to
  // render — .NET verifies the real JWT on every request.
  const session = useCookie<SessionUser | null>('session')
  const isLoggedIn = Boolean(session.value)

  if (to.meta.public) {
    // Guest-only pages: bounce an already-authenticated user out.
    return isLoggedIn ? navigateTo('/') : undefined
  }

  if (!isLoggedIn) {
    return navigateTo('/auth')
  }
})
