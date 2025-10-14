# Smart Track App

## API Mock Toggle

This app includes a built-in mock API client with seeded in-memory data for development.

- Seeded items: Milk, Eggs, Flour
- Operations supported: list, create, update, delete
- Data is in-memory and resets on app reload

### Enabling/Disabling the mock

The toggle is controlled by the environment variable `EXPO_PUBLIC_USE_API_MOCK` and the current build mode.

Defaults:
- Development builds (`__DEV__ === true`): mock is ON by default
- Production builds: mock is OFF by default

You can override the default behavior via `.env` (or your build environment):

```bash
# path=null start=null
# Disable mock in development (use real API)
EXPO_PUBLIC_USE_API_MOCK=false

# OR enable mock in production or previews
EXPO_PUBLIC_USE_API_MOCK=true
```

Notes:
- Environment variables read by the app must be prefixed with `EXPO_PUBLIC_`.
- After changing `.env`, restart the Expo dev server for changes to take effect.

### How it works
- The app creates the API client in `src/services/api.ts`.
- When the mock is enabled, it uses `src/services/clients/api-client.mock.ts` (with a small artificial latency).
- When the mock is disabled, it uses the generated `ApiClient` with Axios (`src/services/clients/api-client.ts` and `src/services/clients/http.ts`).

### Verifying which client is active
You can temporarily add a console statement in `src/services/api.ts` to confirm which client is picked, then reload the app:

```ts path=null start=null
console.log("Using API client:", useMock ? "mock" : "real");
```

Remove the log after verifying.
