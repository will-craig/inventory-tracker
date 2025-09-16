// src/data/gen.ts
import { Configuration, ItemsApi } from "@/src/generated/api";
import { api } from "./client";

const cfg = new Configuration({ basePath: process.env.EXPO_PUBLIC_API_URL });
export const itemsApi = new ItemsApi(cfg, undefined, api); // pass axios instance
