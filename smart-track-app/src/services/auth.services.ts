import { api } from "../../index";
import { http } from "./clients/http";
import { LoginRequest } from "./clients/api-client";
import {AuthStore} from "../auth/auth-store";

export const AuthService = {
  
  async login(username: string, password: string) {

    console.log("Logging in with", username, password);
    console.log('[HTTP baseURL]', http.defaults.baseURL);

    const controller = new AbortController();
    const loginRequest = new LoginRequest({username, password});
    const token = await api.login(loginRequest, controller.signal);
    
    if (!token) throw new Error('No token returned from login');
    await AuthStore.set(token);
  },

  async logout() {
    await AuthStore.clear();
  },
};
