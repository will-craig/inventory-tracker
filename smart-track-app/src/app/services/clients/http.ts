// src/api/http.ts
import axios from "axios";
import { Platform } from "react-native";

const baseUrl = Platform.select({
  android: __DEV__ ? "http://10.0.2.2:8080" : "http://localhost:8080",
  ios: __DEV__ ? "http://localhost:8080" : "http://localhost:8080",
  default: "http://localhost:8080",
});

export const http = axios.create({
  baseURL: baseUrl,
  timeout: 15000,
});

let token: string | null = null;
export const setAuthToken = (t: string | null) => {
  token = t;
};

http.interceptors.request.use((cfg) => {
  if (token) cfg.headers = { ...cfg.headers, Authorization: `Bearer ${token}` };
  return cfg;
});
