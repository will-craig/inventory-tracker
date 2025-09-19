import { registerRootComponent } from 'expo';
// src/api/index.ts
import { ApiClient } from './src/services/clients/api-client';
import { http } from './src/services/clients/http';
import App from "./src/App";

export const api = new ApiClient(http.defaults.baseURL!, http);

// registerRootComponent calls AppRegistry.registerComponent('main', () => App);
// It also ensures that whether you load the app in Expo Go or in a native build,
// the environment is set up appropriately
registerRootComponent(App);
