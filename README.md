# ThingRecommender

Person A recommends a film, TV show, book, comic, or restaurant to Person B. Person B rates it once
they've experienced it. Over time this builds a recommendation-strength score between the two people —
how confidently you can trust Person A's taste, specifically for you.

## Architecture

- **backend/** — .NET 10 API, Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Api`)
- **frontend/** — React + TypeScript (Vite)

The frontend talks to the backend only through the API, so other clients (mobile, etc.) can be added later.

## Backend

Uses SQLite — no separate database server needed. The database file (`thingrecommender.db`) is created
automatically based on the `Database` connection string in
`backend/src/ThingRecommender.Api/appsettings.Development.json`.

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

Auth is currently stubbed with test users — Google/Microsoft sign-in is planned (see `User.ExternalProvider`
/ `User.ExternalId` on the `User` entity). Two seeded test users are always present after migrations run:
`Alice` (`11111111-1111-1111-1111-111111111111`) and `Bob` (`22222222-2222-2222-2222-222222222222`),
see `ThingRecommender.Domain.Seed.SeedUserIds`.

- `POST /api/recommendations` — create (finds-or-creates the `Thing` by title + media type)
- `GET /api/recommendations` — list all
- `POST /api/recommendations/{id}/rate` — rate a recommendation (`{ "score": 1-10 }`)
- `GET /api/recommendations/strength?recommenderId=&recipientId=` — the recommendation-strength score
  for that pair: the average of the scores the recipient has given that recommender so far (`null` /
  count `0` if nothing's been rated yet)

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

## Frontend

```bash
cd frontend
npm install
npm run dev
```

Copy `.env.example` to `.env` and adjust `VITE_API_BASE_URL` if the API isn't running on the default port.
