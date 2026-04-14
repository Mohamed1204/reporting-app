import { useFetch } from '@vueuse/core'
import type { UseFetchOptions } from '@vueuse/core'

export function useApiFetch<T = any>(url: string, options: UseFetchOptions = {}) {
  return useFetch(url, {
    ...options,
    beforeFetch({ options: fetchOptions }) {
      const token = localStorage.getItem('token')
      if (token) {
        fetchOptions.headers = {
          ...fetchOptions.headers,
          Authorization: `Bearer ${token}`,
        }
      }
      return { options: fetchOptions }
    },
  }).json<T>()
}

export async function apiFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const token = localStorage.getItem('token')
  return fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers as Record<string, string> | undefined),
    },
  })
}
