import { beforeAll, describe, expect, it, vi } from 'vitest'
import { createError } from 'h3'

// `server/utils/dotnet.ts` calls createError without importing it — Nitro
// injects it as a global at build time. Vitest runs the file as plain Node,
// where that global does not exist, so it has to be provided here. Handy
// reminder that auto-imports are build magic, not real imports.
let apiError: typeof import('../server/utils/dotnet')['apiError']

beforeAll(async () => {
  vi.stubGlobal('createError', createError)
  ;({ apiError } = await import('../server/utils/dotnet'))
})

/** What ofetch throws: an Error carrying the upstream status. */
function upstream(status: number) {
  return Object.assign(new Error(`HTTP ${status}`), { status })
}

describe('apiError', () => {
  it('passes the upstream status through', () => {
    expect(apiError(upstream(403)).statusCode).toBe(403)
  })

  it('uses a sensible default message per status', () => {
    expect(apiError(upstream(401)).statusMessage).toBe('Not authenticated')
    expect(apiError(upstream(404)).statusMessage).toBe('Not found')
  })

  it('lets the caller override a status message', () => {
    const err = apiError(upstream(404), { 404: 'Period not found' })
    expect(err.statusMessage).toBe('Period not found')
  })

  it('falls back for a status nobody named', () => {
    expect(apiError(upstream(418)).statusMessage).toBe('Upstream API rejected the request')
  })

  // The distinction that caused a misleading 502 back in Phase 3: no `.status`
  // means no response arrived at all, which is the only case 502 describes.
  it('turns a transport failure into 502, not the upstream status', () => {
    const err = apiError(new Error('connect ECONNREFUSED'))

    expect(err.statusCode).toBe(502)
    expect(err.statusMessage).toBe('No response from the reporting API')
  })
})
