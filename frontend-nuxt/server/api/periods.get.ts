import type { FetchError } from 'ofetch'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)

  try {
    return await $fetch<ReportingPeriod[]>(`${config.apiBase}/api/reportingperiods`)
  } catch (err) {
    const e = err as FetchError

    if (e.status) {
      throw createError({
        statusCode: e.status,
        statusMessage: 'Upstream API rejected the request'
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
