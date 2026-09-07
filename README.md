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

Auth is currently stubbed with test users — Google/Microsoft sign-in is planned (see `User.ExternalProvider`
/ `User.ExternalId` on the `User` entity).

## Frontend

```bash
cd frontend
npm install
npm run dev
```

Copy `.env.example` to `.env` and adjust `VITE_API_BASE_URL` if the API isn't running on the default port.
