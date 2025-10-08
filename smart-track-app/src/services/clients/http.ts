// src/api/http.ts
import axios from "axios";
import { Platform } from "react-native";
import { getAccessToken as svcGetAccessToken } from "../auth-service";

const devBase =
  Platform.OS === 'android' ? 'de:a0:db:98:a1:d5:8080' :
  Platform.OS === 'ios'     ? 'https://localhost:8080'  :
  Platform.OS === 'web'     ? 'https://localhost:8080' : 
  'https://192.168.1.10:8080';

export const http = axios.create({ baseURL: devBase, timeout: 15000 });

http.interceptors.request.use(async (config) => {
  const token = await svcGetAccessToken(); // or use context via a wrapper if you prefer
  if (token) {
    config.headers = config.headers ?? {};
    (config.headers as any).Authorization = `Bearer ${token}`;
  }
  return config;
});
