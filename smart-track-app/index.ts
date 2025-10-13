import { registerRootComponent } from 'expo';
import { http } from './services/clients/http';
import {ApiClient} from "./services/clients/api-client";
import RootLayout from "./App/_layout";

export const api = new ApiClient(http.defaults.baseURL!, http);
registerRootComponent(RootLayout);
