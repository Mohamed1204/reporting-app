# Nuxt Learning Track

Learning Nuxt 4 by building a second frontend for the OSS/VAT reporting API,
alongside the existing Vue 3 SPA in `../frontend`. Same backend, same domain —
so the learning goes into Nuxt, not into the problem.

Target archetype: **SSR frontend over an existing API** (the common enterprise /
consultancy shape). Not full-stack Nitro — .NET stays the system of record.

## Progress

- [x] **Phase 1 — Structure & routing**
- [x] **Phase 2 — Data fetching**
- [x] **Phase 3 — The BFF (server routes)**
- [x] **Phase 4 — Auth**
- [ ] **Phase 5 — Rendering strategy**
- [ ] **Phase 6 — Test & deploy**
- [ ] **Phase 7 — Public/SEO section (optional)**

---

## Phase 1 — Structure & routing ✅

- [x] `pages/` turns routing on; `<NuxtPage />` is the outlet
- [x] Ported `/`, `/auth`, `/periods/review` from the SPA's routes array
- [x] Dynamic route `periods/[id].vue` + `useRoute()`
- [x] Static segments beat dynamic ones (`/periods/review` wins over `[id]`)
- [x] `layouts/default.vue` + `layouts/auth.vue`, opted in via `definePageMeta`
- [x] `<NuxtLayout>` is what reads `layout` meta — importing the layout
      directly hardcodes it and breaks named layouts
- [ ] Nested routes (`periods.vue` wrapping `periods/*`) — deferred until needed

**Done-when:** can explain where each concern of `../frontend/src/router/index.ts`
went, and the difference between a nested layout and a nested route.

## Phase 2 — Data fetching

Theme: where a fetch executes, how many times, and how the result crosses from
node to browser.

Pre-flight facts (verified against the installed nuxt 4.5.2):

- `server/` lives at the **project root**, sibling of `app/` — not `app/server/`.
  Nuxt 4 moved `srcDir` to `app/` but left `serverDir` at `<rootDir>/server`.
  Wrong location is a silent 404 with no error.
- `useFetch().data` is `Ref<T | undefined>`, not `| null` (`DefaultT = undefined`
  in `node_modules/nuxt/dist/app/composables/asyncData.d.ts:15`).
- `ReportingPeriodDto.Status` has **no** `[JsonConverter(JsonStringEnumConverter)]`
  and `Program.cs` registers no global one — so `status` on the wire is a number
  `0|1|2` (`Open|Closed|Locked`), unlike `SalesEntryDto`/`PaymentDto` which are
  strings. Match the wire, not the nicer shape.

### 2.1 Stub server route + `useFetch`

- [x] `server/api/periods.get.ts` — `defineEventHandler` returning the 3 seeded
      periods; camelCase keys, `status` as a number, dates as `2026-01-01T00:00:00`
- [x] `app/pages/index.vue` — `await useFetch('/api/periods')`, `v-for` over
      `data ?? []`
- [x] Verify: `curl -s http://localhost:3000/ | grep -o '2026-01-01'` matches —
      then the same curl against the SPA on `:5173` returns nothing

The stub data later moved to `server/utils/periods.ts` so both handlers share it.
Nitro auto-imports everything exported from `server/utils/`, so no import line is
needed — and 3.1 then has exactly one place to swap for the real .NET call.

Conventions in play: `api/` mounts under `/api/*`, `.get` scopes the method, and
returning a value *is* the response. Nitro writes the inferred return type into
`.nuxt/types/nitro-routes.d.ts`, which is what types `useFetch` on the page:
end-to-end types with no shared package and no codegen step.

Stub rather than the real API because `Program.cs:72` adds a global
`AuthorizeFilter` — every controller except `AuthController` is 401 without a
token, and tokens are Phase 4.

### 2.2 The payload — the important one

No new features. Three experiments in DevTools.

- [x] Find the periods inside `__NUXT_DATA__` in view-source. The format is
      `devalue`, not JSON — index-addressed so it can round-trip `Date`, `Map`,
      `Set`, and cyclic refs. Locate the data; don't try to read it fluently.
      Confirmed live: repeated values are *shared* by index — period 1's `id` and
      `status` both point at the slot holding `1`, and periods 2 and 3 share the
      slot holding `0`. It's a reference graph, which is why cycles work.
      `$fwseno8aqkvpp` in there is the key `useFetch` derived from the URL.
- [x] Count invocations, with a `console.log` in the handler so the node side is
      countable and not just the browser tab:

      | action                   | handler | browser XHR |
      |--------------------------|---------|-------------|
      | hard refresh `/`         | 1       | **0**       |
      | client nav `/auth` → `/` | 1       | 1           |

- [x] Build the counter-example: the same fetch in `onMounted` with `$fetch`.
      Renders `null` in curl output — `onMounted` never fires during SSR, so the
      data isn't there when the HTML ships.

**Done-when:** can name which mistake causes which failure. They are two separate
bugs, not one — the original note here conflated them:

| where | what | server fetch | client fetch | in the HTML? |
|-------|------|--------------|--------------|--------------|
| setup | `useFetch` | yes | no — reads payload | yes |
| setup | `$fetch` | yes | **yes, again** | yes |
| `onMounted` | `$fetch` | no | yes | **no** |

`onMounted` gives the empty first paint. `$fetch` **in setup** gives the double
fetch — setup runs on the server *and* again during hydration, and with no
payload entry to read, it repeats the call. Two hits on the backend per view.

So the argument for `useFetch` is not "it works under SSR" — `$fetch` does too.
It's that `useFetch` *hands the result forward* so the second execution is free.

### 2.3 `useFetch` vs `useAsyncData` vs `$fetch`

Do this on a scratch `app/pages/lab.vue`, not in `index.vue`.

`$fetch` is just an HTTP client (ofetch). `useAsyncData` is the SSR-aware, keyed
wrapper around any async fn. `useFetch` = `useAsyncData` + `$fetch`, with the key
derived from URL + options.

Confirmed live in `app/pages/lab.vue` — all three against the same endpoint, then
`curl -s http://localhost:3000/lab | grep -o '<li[^>]*>[^<]*'`:

```
useFetch: 3
useAsyncData: 3
$fetch in onMounted: null
```

Two more things that fell out of it:

- **Setup runs twice in every case** — once on the server, once during hydration.
  What differs is whether the second run touches the network.
- **The key is the dedup unit, not the URL.** `useFetch` and `useAsyncData`
  pointing at the same endpoint under *different* keys both hit the backend. Give
  them the same key and it collapses to one call.

- [ ] `server: false` — data leaves `__NUXT_DATA__` and becomes a browser
      request. This is the Phase 5 escape hatch.
- [ ] `lazy: true` — navigation stops blocking, so it needs `default: () => []`
- [ ] `immediate: false` + `execute()`
- [ ] `watch` — the bridge to 2.4
- [ ] `transform` / `pick` — these shrink what gets serialized; diff the HTML
      size before and after
- [ ] Prefer `status` (`'idle'|'pending'|'success'|'error'`) over the older
      `pending` boolean, which can't tell "not started" from "in flight"
- [ ] Write the three-way comparison into this file in my own words

Rules of thumb: setup + needed for render → `useFetch`; user action → `$fetch`;
not a single URL, or composing several calls → `useAsyncData`.

The option experiments above are **deliberately left unticked** — skipped in
favour of getting to 2.4. `server: false` is the one to come back to, since 5.1
depends on understanding it.

### 2.4 Dynamic route fetching

- [x] `server/api/periods/[id].get.ts` — Nitro does dynamic segments too.
      `getRouterParam(event, 'id')` returns a **string**, so coerce it; use
      `createError({ statusCode: 404 })` for unknown ids (warm-up for 3.2).
      Non-integer ids get a 400, which mirrors what ASP.NET model binding does
      for `GetById(int id)` — a bad id is a malformed request, a valid-but-absent
      id is a 404. Two different failures, two different codes.
- [x] Fetch on `[id].vue` off `route.params.id` — pass the URL as an arrow
      function, not a plain string, so it stays reactive even without a remount
- [x] Break it on purpose: `useAsyncData` with a hardcoded key. Confirmed — the
      heading updates, the dates don't, and the handler is never called.
- [x] The `?tab=` case: query doesn't remount, so nothing re-runs. Correct here,
      because `tab` is view state (`computed` off `route.query`, no fetch).
- [ ] Delete the football lineups from `[id].vue`

### The one idea behind all of 2.4

A fetch re-runs only if it passes **two independent gates**.

**Gate 1 — did the component remount?** Decided by the URL *path*, params
included. `<NuxtPage>` keys pages on `/periods/2`; the query string is not part
of that key. Path changes → Vue destroys and rebuilds → `setup` runs again.
Query changes → same key, same instance → **`setup` never runs**, so no fetch
code is even reached.

**Gate 2 — did the key change?** If setup did run, Nuxt looks up the call's key.
Unchanged key → serve the cached value, skip the network.

Both must open for a request to happen:

| what changed | remount? | key changed? | fetch? | observed |
|---|---|---|---|---|
| `/2` → `/3`, `useFetch` | yes | yes — derived from URL | **yes** | correct dates |
| `/2` → `/3`, key `"period"` | yes | **no** | **no** | heading 3, dates from 2 |
| `?tab=sales` → `?tab=vat` | **no** | never reached | **no** | tab text only |

Rows 2 and 3 both show "no fetch" for *completely different reasons, at
different gates*. That's the part worth keeping.

Falls out of it:

- Use `useFetch` — the URL-derived key makes gate 2 look after itself.
- `useAsyncData` on a dynamic route → the key must carry the varying part:
  `` `period-${route.params.id}` ``.
- A query param that must drive a fetch → `watch: [() => route.query.x]`,
  because gate 1 will never open on its own.

**Done-when:** for any given URL change, can say whether `setup` re-runs, whether
a fetch fires, and where. ✅

**Phase exit:** commit per task rather than one phase-sized blob — it gives
something to `git show` when writing the SPA comparison later.

## Phase 3 — The BFF

Theme: the server route stops being a stub and becomes a seam. Three tiers —
browser → Nitro → .NET → SQL Server — and the browser never learns .NET exists.

- [x] 3.1 Replace the stub body with a real proxy to .NET using the *private*
      `runtimeConfig.apiBase`
- [x] 3.2 Propagate .NET errors sensibly (`createError`)
- [x] 3.3 Articulate what this bought: no CORS, no cert problem, secrets stay server-side

### What actually changed

Only `server/api/periods.get.ts` and `server/api/periods/[id].get.ts`.
`index.vue` and `[id].vue` were not touched and still type-check — they just
render SQL Server rows now instead of the hardcoded array. That is the seam:
the Vue layer is coupled to *our* API contract, not to .NET's.

`server/utils/periods.ts` kept the `ReportingPeriod` interface and lost the fake
array. The interface still earns its place, because `$fetch(url)` with no
generic returns `any` — drop `$fetch<ReportingPeriod[]>` and the end-to-end
typing from Phase 2 dies silently.

### Where the "call the BFF instead of .NET" decision lives

Nowhere but the URL. Both shapes are in the repo right now:

| file | call | goes to |
| `app/pages/index.vue` | `useFetch("/api/periods")` | Nitro — relative URL |
| `app/pages/auth.vue` | `$fetch(path, { baseURL: config.public.apiBase })` | .NET, direct |

Omit `baseURL` → relative → the browser resolves against the document's origin →
whoever served the page. Set it → absolute → straight to Kestrel. There is no
switch or middleware; Nuxt never "routes" the call. `/api/periods` reaches the
handler only because a file sits at that path — delete the file and the same
line silently returns HTML from the page renderer.

The browser needs no config for hop 1 because it is same-origin (the address bar
*is* the config). Only the cross-origin hop needs an address, which is why
`apiBase` exists and why CORS disappeared.

### Hops, and where the extra tier costs anything

SSR: `useFetch` to our own route is an **in-process call** — Nitro invokes the
handler directly, no loopback socket. One real outbound request, same as the SPA.
Client-side nav: two real hops. So the extra tier is paid on navigation, not on
first paint — and in production the BFF→API hop is inside the VPC while
browser→BFF crosses the internet, which is why aggregating several API calls
into one BFF endpoint usually *wins* latency despite adding a tier.

### The argument, both directions

For: CORS gone (`UseCors("VueFrontend")` is now dead weight on the Nuxt path);
the cert problem became a config decision instead of a browser warning;
`apiBase` never reaches the page (grep the HTML — no `5247`); and response
shaping becomes possible (`status: 0` → `"Open"` belongs in the handler, not
duplicated in two `.vue` files).

Against: periods are public, cacheable, read-only — a CDN could serve them.
Instead there are two hops, a Node process to operate, and error semantics
written twice in two languages.

Rule: a BFF earns its place when there is a secret to hold, a token to attach,
calls to aggregate, or a payload to reshape. `/api/periods` is the weak case
today and becomes the strong case the moment Phase 4 puts a JWT on it.

**Done-when:** can argue both for and against the BFF for a given endpoint. ✅

### Left behind for Phase 4

- `runtimeConfig.public.apiBase` still ships the .NET origin to the browser.
  It exists only for `auth.vue`. When that call moves behind the BFF, delete the
  whole `public` block and `.env.example`.
- `[AllowAnonymous]` on the two GETs in `ReportingPeriodsController` is temporary
  scaffolding — Phase 4 removes it.
- `app/layouts/default.vue` links to `/periods/3`, but the real DB only has ids
  1 and 2. Good for seeing `createError` render as an error page; fix afterwards.

## Hard constraint — the SPA must keep working

`frontend/` (Vue SPA) and `frontend-nuxt/` both hit the same .NET API, and
`AuthController` is shared. Keeping both alive is deliberate: the SPA is the
control group for the comparison write-up at the end.

**Rule: Phase 4 changes Nuxt, not .NET.** Nitro adapts to what `AuthController`
already does — `HttpOnly`, `SameSite=Strict`, cookie name `refreshToken`,
`Secure = !IsDevelopment()`. Do not edit `SetRefreshCookie`/`ClearRefreshCookie`
to make the BFF more convenient. That is also the realistic constraint: a BFF is
normally built against an API you do not own. If a change there is genuinely
unavoidable, add a new endpoint rather than modifying an existing one.

**There is no automated safety net.** `src/stores/__tests__/auth.spec.ts` is the
SPA's only spec and does not exercise `fetch`; `ReportingApi1.Tests` covers
services only, no controllers. Nothing asserts the API contract. So verify by
hand after any .NET-side change:

1. `cd frontend && npm run dev` → log in at `:5173`
2. Load a page that lists periods, and one that needs the token
3. Hard-refresh a protected page — the refresh-cookie flow must still fire
4. Log out, confirm the cookie is cleared

Changes made so far, and why neither touches the SPA:

- `[AllowAnonymous]` on the two `ReportingPeriods` GETs — additive. The SPA still
  sends its JWT; the endpoint just stopped requiring one. Removing it at the end
  of Phase 4 restores the original behaviour exactly.
- `UseHttpsRedirection` gated to non-development — the SPA's Vite proxy targets
  `https://localhost:7033` directly (`vite.config.ts:19`) and never touches 5247.
  The HTTPS listener is unchanged.

**The SPA's Vite proxy is a reverse proxy, not a BFF.** `/api` → `:7033` with
`secure: false` looks like the same three tiers, but it is dev-only (nginx does
it in prod — a different component with a different config) and it is a dumb
pipe: it cannot hold a secret, reshape a response, aggregate calls, or keep the
token out of browser JS. Reverse proxy and BFF are identical on a diagram; the
difference is whether the middle tier can run your logic.

## Phase 4 — Auth (the hard one)

Rewrites the login page currently in `app/pages/auth.vue`.

- [x] 4.0 Move login behind the BFF — delete `baseURL:` from `auth.vue:32`, add
      `server/api/auth/login.post.ts`. Prerequisite for everything below: today
      .NET sets the refresh cookie on *its own* origin, where Nitro cannot see it.
- [x] 4.1 Token in a cookie instead of `useState`. Two options — (a) a normal
      cookie readable by JS, or (b) **HttpOnly, set by Nitro, token never reaches
      the browser**. Going with (b): it is the reason the BFF pattern exists.
      Note HttpOnly can only be set server-side, so this is h3's
      `setCookie(event, ...)` in the handler, not `useCookie` in Vue.
- [x] 4.2 Route middleware, replacing the SPA's `router.beforeEach` guard.
      Built as **default-deny**: `app/middleware/auth.global.ts` runs on every
      route, pages opt *out* with `definePageMeta({ public: true })`. Keys on the
      non-HttpOnly `session` cookie, never `token` — `useCookie` cannot see an
      HttpOnly cookie, so keying on `token` would look logged out on every
      client-side navigation. Safe, because this only decides what to *render*;
      .NET still verifies the real JWT.
- [x] 4.3 **Cookie forwarding on server-side fetches.** Turned out to be half
      solved already: `useFetch` with a relative `/`-prefixed URL swaps the global
      `$fetch` for `useRequestFetch()`, which is `event.$fetch`, which merges
      `getProxyRequestHeaders(event)` — and `cookie` is *not* in h3's strip list.
      So hop 2 (page render → own API route) already carries it. The real work was
      hop 3: `server/utils/dotnet.ts` translating the HttpOnly cookie into
      `Authorization: Bearer` for .NET. That swap *is* the BFF.
- [x] 4.4 Refresh-token flow + logout. Not `SameSite` — that keys on *site*
      (registrable domain), so `:3000` and `:7033` were already same-site. The
      work was that .NET's `Set-Cookie` now returns to **Nitro**, not the
      browser: `$fetch.raw()` to reach the header, then re-issue the refresh
      token as our own cookie. Three things only showed up under test —
      a rotating-token API needs a **grace window**, not just single-flight;
      **logout has to invalidate that cache**; and the refresh has to happen in
      **server middleware on the real request**, because a sub-request's
      Set-Cookie is discarded. See the gotchas.
- [x] 4.5 Removed `[AllowAnonymous]` from `ReportingPeriodsController` — done
      with 4.3, because until the endpoint is protected, attaching the bearer
      header changes nothing observable and "verified" would mean nothing.

**Done-when:** can trace a hard refresh of a protected page end to end, naming
where every line executes.

## Phase 5 — Rendering strategy

- [ ] 5.1 `routeRules`: prerender public, SSR some, `ssr: false` for the dashboard
- [ ] 5.2 `error.vue`, `createError`, `showError`
- [ ] 5.3 `useSeoMeta` on anything public

**Done-when:** can justify each rule to a tech lead.

## Phase 6 — Test & deploy

- [ ] 6.1 `@nuxt/test-utils` + Vitest
- [ ] 6.2 Dockerfile — node runtime, no nginx stage (contrast `../frontend/Dockerfile`)
- [ ] 6.3 Add as a service in `../docker-compose.yml`

## Phase 7 — Public/SEO section (optional)

- [ ] Prerendered marketing/docs pages + `@nuxtjs/i18n` — covers the
      headless-CMS/SEO archetype most Nuxt job ads assume

---

## Gotchas learned (don't relearn these)

- **Adding a file to `server/` needs a dev-server restart.** HMR covers `app/`
  but not a directory Nitro wasn't watching when it started. Symptom is a 404
  that looks like a wrong file path. Tell them apart by the PID: if `netstat -ano
  | grep :3000` still shows the old process, nothing was reloaded. Confirmed by
  `.nuxt/types/nitro-routes.d.ts` — a registered route appears there as
  `ReturnType<typeof import('../../server/api/…')>`, and that declaration is also
  what types `useFetch` on the page.
- **Module scope vs handler body.** A `console.log` above
  `export default defineEventHandler(...)` fires **once per process**, at import
  time — not per request. Per-request code goes *inside* the handler. Same
  distinction as .NET's `Scoped` vs singleton: connection pools and caches belong
  at module scope, per-request state does not.
- **An unmatched `/api/*` path falls through to the page renderer**, not to a
  plain 404. Nitro checks `server/` routes first; if none match, the Vue app gets
  the request and 404s from `pages/runtime/plugins/router.js`. So a typo'd
  handler filename fails quietly as a missing *page*, not a missing route.
- **`params` remount, `query` reuses.** `<NuxtPage>` keys pages on the matched
  path with params interpolated (`node_modules/nuxt/dist/pages/runtime/utils.js`),
  so a param change remounts and re-runs `setup`, but a query change does not.
  Plain vue-router reuses in *both* cases.
- **`runtimeConfig.apiBase` is `undefined` in the browser.** Only usable from
  `server/` code. Page components run in both places, so they must use
  `config.public.apiBase` — or better, call a server route.
- **Node rejects the ASP.NET dev cert.** Server-side fetches must use the `http`
  profile (`http://localhost:5247`); the browser can use `https://localhost:7033`.
- **`useState` does not persist.** Hard refresh clears it, and anything in it is
  serialized into the SSR payload — never put a token there.
- **Importing a layout instead of using `<NuxtLayout>`** silently disables
  `definePageMeta({ layout })`.

- **`UseHttpsRedirection` turns the .NET HTTP port into a 307 to HTTPS**, and
  `$fetch` follows redirects. So pointing the BFF at `http://localhost:5247`
  does *not* dodge the dev certificate — the redirect walks straight into it and
  Node throws `DEPTH_ZERO_SELF_SIGNED_CERT`. Symptom is confusing because a
  plain `curl` to 5247 prints *nothing*: a 307 has an empty body and curl
  without `-L` doesn't follow. Always `curl -i` when a request "returns nothing".
  Fix: gate the redirect to non-development in `Program.cs` — it exists to stop
  *browsers* sending credentials in plaintext and does nothing for
  service-to-service calls, where TLS terminates at the ingress anyway.

- **A catch-all error branch will confidently tell you the wrong cause.**
  `e.status ?? 502` with the message "API is unreachable" was inferred from the
  *absence* of a status — but TLS failures, DNS failures and timeouts all take
  that branch, not just connection-refused. Split it: pass through `e.status`
  when there is one, and for the no-response case say only what is known
  ("no response from the API") with the real `e.message` in `data`, gated on
  `import.meta.dev`. Put the cause in `data`, never `statusMessage` — that
  becomes the HTTP reason phrase, which cannot hold newlines or non-ASCII, and
  Node throws `ERR_INVALID_CHAR`.

- **A build that fails inside an MSBuild *task* means `obj/`, not your code.**
  `DefineStaticWebAssets` threw `'0x00' is an invalid start of a value` because
  two `*.dswa.cache.json` files were null-filled by an interrupted write.
  Deleting them fixed it. Also: `MSB3021`/`MSB3027` "file is locked" just means
  the API is still running — Ctrl+C it. Neither is a compile error; look for
  `CS####` before suspecting your source.

- **`useFetch` forwards cookies during SSR. Bare `$fetch` does not.** The failure
  mode is "works when you click a link, 401s on refresh", because a client-side
  navigation runs in the browser, which attaches cookies itself — so dev looks
  fine until someone hits F5. `useFetch` only gets the special treatment when the
  URL is a string starting with `/` and there is no absolute `baseURL`
  (`nuxt/dist/app/composables/fetch.js:108`); it then uses `useRequestFetch()`,
  which is `event.$fetch`, which is h3's `fetchWithEvent` merging
  `getProxyRequestHeaders(event)`. The strip list there is `transfer-encoding,
  accept-encoding, connection, keep-alive, upgrade, expect, host, accept` —
  `cookie` is not on it. So `useAsyncData(() => $fetch('/api/x'))` silently
  drops the credential on SSR; `useAsyncData(() => useRequestFetch()('/api/x'))`
  does not.

- **Scaffolding you added to unblock yourself is not a constraint to design
  around.** Before touching `[AllowAnonymous]`, check who put it there:
  `git show <commit-before>:<file>` settled in one command that the attribute was
  mine from Phase 3, not part of the original app. Same habit for "will this
  break the other frontend?" — `grep -rhoE "api/[A-Za-z0-9_/-]+" frontend/src`
  listed every endpoint the SPA calls and `ReportingPeriods` was not among them.
  Two greps beat an argument about risk.

- **h3 does not share `context` into in-process SSR sub-requests.** Verified
  with a probe, not assumed: a page render logged one context id and its own
  `/api/periods` call logged a different one. The consequence is sharp — a
  sub-request has its own response object, which is thrown away once the body
  is read, so **`setCookie` inside an API handler during SSR reaches nobody**.
  It works perfectly on a client-side call to the same route, so this only
  breaks on hard refresh. Anything that must reach the browser has to run in
  `server/middleware/`, on the real request. And after changing the session
  there, rewrite `event.node.req.headers.cookie` too — Nitro copies those
  headers into its sub-requests at fetch time, so that is how the fresh token
  gets forwarded.

- **Single-flight is not enough for rotating refresh tokens; you need a grace
  window.** De-duplicating *concurrent* refreshes is the obvious half. The half
  that bites: a request arriving a moment after the first refresh settles is
  still carrying the old cookie — its copy was taken before the rotation — and
  spending it again is exactly the replay the API punishes. Measured: 6 parallel
  requests produced 2 refresh calls and one `reuse detected`, revoking the
  family and 401ing four of them. Keep resolved entries for ~10s and hand back
  the same result. Then **logout must purge that cache**, or a spent token can
  fetch the revoked session straight back out of it.

- **Logout cannot revoke an access token.** Stateless JWT: .NET checks signature
  and `exp`, both still valid, so a *captured* token keeps working until it
  expires. Logout revokes the refresh token and clears the cookies — that is the
  whole of what it can do. This is the argument for `ExpiryMinutes: 15` in
  production; dev's 600 makes the window ten hours.

- **`worker exited with code 0` from the Nitro dev server is not your bug.**
  Editing anything in `server/utils/` triggers an HMR reload that sometimes
  kills the dev worker outright, after which *every* route 500s with that
  message — including ones that have nothing to do with the change. Restart the
  dev server before debugging. Two of the failures chased in 4.4 were this.

## Deployment & architecture (settled — don't relearn)

**Why `curl` differs.** `curl` has no JS engine, so it shows exactly what went on
the wire. The SPA's `index.html` is a literal file with an empty
`<div id="app">`; nginx reads it off disk and sends it, and the `<h1>` is only
created later by `createApp().mount()` in the browser. Nuxt's node process
executes the component via `renderToString` (`vue/server-renderer`) and sends
HTML that already contains the markup, then the browser hydrates.

**"Server" is overloaded.** nginx and the Vite dev server are both server
processes, but neither has a JS engine — neither can execute a `.vue` file. Only
node + `.output/server` can. That's the only distinction that matters for SSR.

**Build output differs in kind.**

| | produces | needs at runtime |
|---|---|---|
| SPA | `dist/` — just files | a file server (or a bucket) |
| Nuxt | `.output/public/` **+** `.output/server/` | a node process |

Static files can go to S3/CDN. A node server can't — something must execute code
per request. Hence `../frontend/Dockerfile` discards node and ends at
`nginx:alpine` (~15 MB idle), while Nuxt needs `node .output/server/index.mjs`
(~100 MB per replica, real CPU per request).

**nginx in the SPA is a dumb pipe.** `../frontend/default.conf.template` has two
`location` blocks and nothing more: `/` reads from disk, `/api/` forwards to
`${BACKEND_URL}`. It never sees inside a request. `dist` goes to the *browser* —
.NET never receives it. The `envsubst`/`${BACKEND_URL}` dance exists only because
Vite bakes env vars at build time; node reads `process.env` natively, which is
what `runtimeConfig` is wired to.

**Who can call .NET.** SPA: browser only — so reaching the API means either CORS
(local dev: `Program.cs:58` allows `:5173`) or same-origin via the nginx proxy
(what `docker-compose.yml` does). Nuxt adds a third caller: the node server,
which can fetch server-to-server before the browser is involved.

**Prod topology — three processes, one public.**

```
internet ──> node :3000 ──> .NET :8080 ──> SQL :1433
             (public)       (private)      (private)
```

`NUXT_API_BASE=http://api:8080` over the internal network. The browser never
calls .NET, so CORS is unnecessary in prod. The SPA's topology is identical with
nginx in the public slot — the diagram barely changes. What changes is that the
public box now runs application code: **the middle box goes from a pipe to a
program.**

**Serving the frontend — three normal options.** (a) `dist` into ASP.NET
`wwwroot` + `UseStaticFiles()` — one deployable, common in .NET-centric teams;
(b) a separate static host, i.e. the current nginx container — independent
deploys, generalises best; (c) no process at all — S3+CloudFront / Cloudflare
Pages / Azure SWA, probably the most common in real cloud prod. Going (b)→(c)
costs the nginx `/api/` proxy, so the proxying moves into the provider's routing
config (CloudFront behaviors, `_redirects`, `staticwebapp.config.json`).

**Nitro presets** are genuinely Nuxt-only — one source, many runtime artifacts:
`nuxt build` (node), `--preset=cloudflare_module`, `--preset=aws-lambda`,
`--preset=static` (pure files, deployable to a CDN). A plain SPA has exactly one
possible artifact. With `routeRules` the modes mix in a single build: prerender
public pages to a CDN, SSR or `ssr: false` the login-gated dashboard. No
equivalent in `../frontend`, where every route shares one strategy.

## Running it

```bash
# API (plain HTTP so Node can call it)
cd ../ReportingApi1 && dotnet run --launch-profile http

# Nuxt
npm run dev          # http://localhost:3000
```

Dev credentials from `../ReportingApi1/Infrastructure/DbSeeder.cs`: `admin` / `admin`

## After the phases

Competence comes from the phases; skill comes from repetition. The highest-value
follow-ups, in order: ship it to production, build a differently-shaped app
(public + CMS), write a small Nuxt module or layer, do a version upgrade.

Also worth doing after Phase 4: write an honest comparison of this app against
`../frontend` — what got simpler, what got worse, what to choose next time.
