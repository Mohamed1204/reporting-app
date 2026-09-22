import { describe, expect, it } from 'vitest'
import { readJwtExpiry } from '../server/utils/jwt'

/** Builds a token with the given payload. Only the middle segment is ever read. */
function token(payload: object) {
  const body = Buffer.from(JSON.stringify(payload)).toString('base64url')
  return `header.${body}.signature`
}

describe('readJwtExpiry', () => {
  it('reads exp', () => {
    expect(readJwtExpiry(token({ exp: 1788993270 }))).toBe(1788993270)
  })

  it('returns null when the token carries no exp', () => {
    expect(readJwtExpiry(token({ userName: 'admin' }))).toBeNull()
  })

  // The cookie lifetime is derived from this, so a non-numeric exp must not
  // sneak through as NaN and produce a cookie that expires immediately.
  it('returns null when exp is not a number', () => {
    expect(readJwtExpiry(token({ exp: 'soon' }))).toBeNull()
  })

  it('returns null for malformed tokens rather than throwing', () => {
    expect(readJwtExpiry('not-a-jwt')).toBeNull()
    expect(readJwtExpiry('header..signature')).toBeNull()
    expect(readJwtExpiry('header.@@@not-base64@@@.signature')).toBeNull()
  })
})
