import { api } from "../../../index";
import { setAuthToken } from "./clients/http";
import { LoginRequest } from "./clients/api-client";

export const AuthService = {
  async login(username: string, password: string) {
    const controller = new AbortController();
    const token = await api.login(
      LoginRequest.fromJS({ username, password }),
      controller.signal,
    );
    setAuthToken(token);
  },

  logout() {
    setAuthToken(null);
  },
};
