import { ApiClient } from "./clients/api-client";
import { http } from "./clients/http";

// Central API client instance used across services
export const api = new ApiClient(http.defaults.baseURL ?? "", http);
