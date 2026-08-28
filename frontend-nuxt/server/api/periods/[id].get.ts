import type { FetchError } from 'ofetch'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)

  const id = Number(getRouterParam(event, 'id'))

  if (!Number.isInteger(id)) {
    throw createError({ statusCode: 400, statusMessage: 'Period id must be an integer' })
  }

  try {
    return await $fetch<ReportingPeriod>(`${config.apiBase}/api/reportingperiods/${id}`)
  } catch (err) {
    const e = err as FetchError

    if (e.status === 404) {
      throw createError({ statusCode: 404, statusMessage: 'Period not found' })
    }

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
