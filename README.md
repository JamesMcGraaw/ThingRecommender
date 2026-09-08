# ThingRecommender

Person A recommends a film, TV show, book, comic, or restaurant to Person B. Person B rates it once
they've experienced it. Over time this builds a recommendation-strength score between the two people —
how confidently you can trust Person A's taste, specifically for you.

## Architecture

- **backend/** — .NET 10 API, Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **frontend/** — React + TypeScript (Vite)

The frontend talks to the backend only through the API, so other clients (mobile, etc.) can be added later.

## Backend

Uses PostgreSQL — [Neon](https://neon.tech) has a free tier and takes about a minute to set up (sign in
with Google/GitHub, create a project, copy the connection string). Set it via user-secrets rather than
`appsettings.json`, since it contains a real credential:

```bash
cd backend/src/ThingRecommender.Api
dotnet user-secrets set "ConnectionStrings:Database" "Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;Channel Binding=Require"
```

(Neon gives you a `postgresql://user:password@host/db?sslmode=require&channel_binding=require` URI —
convert it to the `Host=...;Username=...` form above; Npgsql doesn't parse the URI form directly.)

```bash
cd backend/src/ThingRecommender.Api
dotnet run
```

Run tests:

```bash
dotnet test ThingRecommender.slnx
```

Migrations use a pinned local `dotnet-ef` tool (see `.config/dotnet-tools.json`):

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update --project backend/src/ThingRecommender.Infrastructure --startup-project backend/src/ThingRecommender.Api
```

To add a new migration after changing entities:

```bash
dotnet tool run dotnet-ef migrations add <Name> --project backend/src/ThingRecommender.Infrastructure --startup-project backend/src/ThingRecommender.Api --output-dir Persistence/Migrations
```

Two seeded test users still exist after migrations run — `Alice` (`alice@example.com`) and `Bob`
(`bob@example.com`) — but they're no longer a login shortcut; every endpoint below requires a real
sign-in (see [Authentication](#authentication)). Signing in with an email matching a seeded user links
your real account to it rather than creating a duplicate, so existing test data carries over once you do.

All endpoints require `Authorization: Bearer <token>` (the token from `POST /api/auth/{provider}`) unless noted:

- `POST /api/recommendations` — create (`{ recipientEmail, thingTitle, mediaType, note? }`). The
  recommender is always the authenticated user, not something the client specifies. 404s if no account
  exists yet for `recipientEmail`. Finds-or-creates the `Thing` by title + media type.
- `POST /api/recommendations/manual` — log a recommendation from someone who isn't a ThingRecommender
  user (`{ externalRecommenderName, thingTitle, mediaType, note? }`). The recipient is always the
  authenticated user; `Recommendation.RecommenderId` is left null and `ExternalRecommenderName` holds the
  free-text name instead. There's no persistent identity behind that name (no account, no email), so it
  can't feed the `/strength` endpoint below — the frontend instead computes a rough per-name average
  client-side from the recommendations already in hand.
- `GET /api/recommendations` — recommendations where you're the recommender or the recipient
- `POST /api/recommendations/{id}/rate` — rate a recommendation (`{ "score": 1-10 }`). 403 unless you're
  the recipient.
- `GET /api/recommendations/strength?recommenderId=&recipientId=` — the recommendation-strength score
  for that pair: the average of the scores the recipient has given that recommender so far (`null` /
  count `0` if nothing's been rated yet). 403 unless you're one of the two people in the pair.

### External links

Newly-created Films and TV shows get looked up on [TMDB](https://www.themoviedb.org/) for an external
link (`Thing.ExternalUrl`), keyed off `MediaType` so other providers (IGDB for `VideoGame`, Google Books
for `Book`/`Comic`, etc.) can be added the same way later. It's optional — with no API key configured the
lookup just returns `null` and recommendation creation still succeeds.

To enable it locally: [get a free TMDB API key](https://www.themoviedb.org/settings/api) (v3 auth), then:

```bash
cd backend/src/ThingRecommender.Api
dotnet user-secrets set "Tmdb:ApiKey" "<your key>"
```

Never put a real key in `appsettings.json` / `appsettings.Development.json` — those are committed to the repo.

### Authentication

Sign-in works like this: the frontend gets an ID token straight from Google/Microsoft's own SDK, POSTs it
to `POST /api/auth/{google|microsoft}`, the backend verifies it (signature, audience, expiry) and returns
its own JWT, which the frontend then sends as `Authorization: Bearer` on every subsequent request. No
OAuth client secret is ever handled by this app — only the public Client ID, which both providers use with
a PKCE/ID-token flow designed for a browser-based app.

**JWT signing key** (already set for local dev in this repo's clone, but any fresh clone needs its own):

```bash
cd backend/src/ThingRecommender.Api
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 32)"
```

**Google** — [console.cloud.google.com/apis/credentials](https://console.cloud.google.com/apis/credentials):
1. Create a project (or pick an existing one), then **Create Credentials → OAuth client ID**.
2. If prompted, configure the OAuth consent screen first (External, add yourself as a test user is enough
   for local dev).
3. Application type: **Web application**. Under **Authorized JavaScript origins**, add
   `http://localhost:5173`. No redirect URI is needed for this flow.
4. Copy the **Client ID** (not the secret — it isn't used):
   ```bash
   dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
   ```
   and in `frontend/.env`: `VITE_GOOGLE_CLIENT_ID=<client-id>`

**Microsoft** — [entra.microsoft.com](https://entra.microsoft.com) → **App registrations → New registration**:
1. Name it anything. Under **Supported account types**, pick "Accounts in any organizational directory
   and personal Microsoft accounts" — this matches the `common` tenant this app uses by default.
2. Under **Authentication → Add a platform → Single-page application**, add redirect URI
   `http://localhost:5173`.
3. Copy the **Application (client) ID**:
   ```bash
   dotnet user-secrets set "Authentication:Microsoft:ClientId" "<client-id>"
   ```
   and in `frontend/.env`: `VITE_MICROSOFT_CLIENT_ID=<client-id>`

Either provider works independently — the sign-in page just shows "not configured" for whichever one has
no Client ID set, rather than breaking.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

Copy `.env.example` to `.env`, adjust `VITE_API_BASE_URL` if the API isn't running on the default port,
and set `VITE_GOOGLE_CLIENT_ID` / `VITE_MICROSOFT_CLIENT_ID` as described above.

## Deployment

**Backend** is deployed on [Render](https://render.com) as a Docker web service, built from the repo-root
`Dockerfile` (multi-stage: publish on the .NET SDK image, run on the smaller ASP.NET runtime image,
listens on `8080`). Live at `https://thingrecommender.onrender.com`.

Config is set as environment variables in the Render dashboard (Environment tab), using the same
double-underscore convention .NET config binding expects for nested keys (e.g.
`ConnectionStrings__Database`, `Jwt__SigningKey`, `Authentication__Google__ClientId`) — see
[Authentication](#authentication) and the connection string note above for what each value should be.
Also set `ASPNETCORE_ENVIRONMENT=Production`. The free instance spins down after inactivity (~50s cold
start on the next request) and needs a card on file for verification, even though it isn't charged.

**Frontend** is deployed on [GitHub Pages](https://pages.github.com) via the
`.github/workflows/deploy-frontend.yml` GitHub Actions workflow, which runs on every push to `master`
that touches `frontend/`. Live at `https://jamesmcgraaw.github.io/ThingRecommender/`.

GitHub Pages requires the repo to be public on the free plan (this repo was made public for that reason —
its history was checked first for anything that shouldn't be, and it was clean). Build-time config comes
from repository variables (Settings → Secrets and variables → Actions → Variables) rather than secrets,
since `VITE_`-prefixed values end up in the public client bundle either way: `VITE_API_BASE_URL`,
`VITE_GOOGLE_CLIENT_ID`, and optionally `VITE_MICROSOFT_CLIENT_ID` / `VITE_MICROSOFT_TENANT_ID`.

`vite.config.ts` sets `base: '/ThingRecommender/'` for production builds only (GitHub Pages project sites
are served under `/<repo>/`) — local dev still runs at the root.

Whenever the frontend's deployed origin changes, it needs to be added in two places: `Cors__AllowedOrigins__0`
in Render's environment variables, and as an **Authorized JavaScript origin** on the Google OAuth client
(see [Authentication](#authentication)) — both already done for the current GitHub Pages URL.
