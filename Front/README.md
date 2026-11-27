# Interface Payroll: front end

Angular 21 client for the Interface Employee & Payroll API.
Setup, architecture and design decisions are in the [main README](../README.md#frontend-architecture).

| Command | What it does |
| --- | --- |
| `npm start` | Dev server on http://localhost:4200 (the API must run on http://localhost:5019) |
| `npm test -- --watch=false` | Unit and component tests (Vitest) |
| `npm run build` | Production build into `dist/` |
| `npm run api:update` | Downloads the API's Swagger document and regenerates `src/app/core/api/generated` (API must be running) |
| `npm run api:generate` | Regenerates the client from the committed `openapi/interface-api.json` |
