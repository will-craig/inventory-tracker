// src/api/http.ts
import axios from "axios";
import { Platform } from "react-native";
import {AuthStore} from "../../auth/auth-store";

const devBase =
  Platform.OS === 'android' ? 'de:a0:db:98:a1:d5:8080' :
  Platform.OS === 'ios'     ? 'https://localhost:8080'  :
  Platform.OS === 'web'     ? 'https://localhost:8080' : 
  'https://192.168.1.10:8080';

export const http = axios.create({
  baseURL: __DEV__ ? devBase : 'https://api.example.com',
  timeout: 15000,
});

http.interceptors.request.use((cfg) => {
  const token = AuthStore.get();
  if (token) {
    (cfg.headers as any).Authorization = `Bearer ${token}`;
  }
  return cfg;
});
