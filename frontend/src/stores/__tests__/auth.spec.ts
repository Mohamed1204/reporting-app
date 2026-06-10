import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'

describe('auth store', () => {
  beforeEach(() => {
    // Each test gets a fresh Pinia so state never leaks between tests.
    setActivePinia(createPinia())
  })

  it('starts logged out with an empty token', () => {
    const auth = useAuthStore()

    expect(auth.token).toBeNull()
    expect(auth.isLoggedIn).toBe(false)
  })

  it('isLoggedIn reflects whether a token is present', () => {
    const auth = useAuthStore()

    auth.token = 'jwt-token'
    expect(auth.isLoggedIn).toBe(true)

    auth.token = null
    expect(auth.isLoggedIn).toBe(false)
  })
})
