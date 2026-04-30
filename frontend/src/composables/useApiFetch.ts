import { useFetch } from '@vueuse/core'
import type { UseFetchOptions } from '@vueuse/core'
import { useAuthStore } from '../stores/auth'
import router from '../router'

let refreshPromise: Promise<boolean> | null = null

async function ensureRefresh(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = useAuthStore()
      .refresh()
      .finally(() => {
        refreshPromise = null
      })
  }
  return refreshPromise
}

export function useApiFetch<T = any>(url: string, options: UseFetchOptions = {}) {
  const auth = useAuthStore()
  return useFetch(
    url,
    { credentials: 'include' },
    {
      ...options,
      beforeFetch({ options: fetchOptions }) {
        if (auth.token) {
          fetchOptions.headers = {
            ...fetchOptions.headers,
            Authorization: `Bearer ${auth.token}`,
          }
        }
        return { options: fetchOptions }
      },
      async onFetchError(ctx) {
        if (ctx.response?.status === 401) {
          const ok = await ensureRefresh()
          if (!ok) router.push({ name: 'auth' })
        }
        return ctx
      },
    },
  ).json<T>()
}

export async function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const make = (): Promise<Response> => {
    const token = useAuthStore().token
    return fetch(url, {
      ...options,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(options.headers as Record<string, string> | undefined),
      },
    })
  }

  let res = await make()
  if (res.status === 401) {
    const ok = await ensureRefresh()
    if (ok) {
      res = await make()
    } else {
      router.push({ name: 'auth' })
    }
  }
  return res
}
