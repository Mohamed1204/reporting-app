interface JwtClaims {
  exp?: number
}

/**
 * Reads the `exp` claim without verifying the signature. Safe here because the
 * token came straight from .NET and is only used to derive a cookie lifetime —
 * never for an authorization decision. .NET verifies it on every request.
 */
export function readJwtExpiry(token: string): number | null {
  const payload = token.split('.')[1]

  if (!payload) {
    return null
  }

  try {
    const claims: JwtClaims = JSON.parse(Buffer.from(payload, 'base64url').toString())
    return typeof claims.exp === 'number' ? claims.exp : null
  } catch {
    return null
  }
}
