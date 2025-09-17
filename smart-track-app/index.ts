import { registerRootComponent } from 'expo';

import App from './App';
// src/api/index.ts
import { ApiClient } from './src/app/services/clients/api-client';
import { http } from './src/app/services/clients/http';

export const api = new ApiClient(http.defaults.baseURL!, http);

// registerRootComponent calls AppRegistry.registerComponent('main', () => App);
// It also ensures that whether you load the app in Expo Go or in a native build,
// the environment is set up appropriately
registerRootComponent(App);
