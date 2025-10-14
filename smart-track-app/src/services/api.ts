import { ApiClient, type IApiClient } from "./clients/api-client";
import { http } from "./clients/http";
import { MockApiClient } from "./clients/api-client.mock";

// Toggle mocking with an env flag. Behavior:
// - In dev (__DEV__), default is mock ON unless EXPO_PUBLIC_USE_API_MOCK is explicitly false/0/no.
// - In prod, default is mock OFF unless EXPO_PUBLIC_USE_API_MOCK is explicitly true/1/yes.
function parseBoolEnv(v?: string) {
  const s = (v ?? "").trim().toLowerCase();
  if (["1", "true", "yes", "on"].includes(s)) return true;
  if (["0", "false", "no", "off"].includes(s)) return false;
  return undefined;
}

const envSetting = parseBoolEnv(process?.env?.EXPO_PUBLIC_USE_API_MOCK as any);
const useMock =
  typeof __DEV__ !== "undefined" && __DEV__
    ? envSetting !== false // default true in dev
    : envSetting === true; // default false in prod

export const api: IApiClient = useMock
  ? new MockApiClient()
  : new ApiClient(http.defaults.baseURL ?? "", http);
