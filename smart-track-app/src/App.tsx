// App.tsx
import * as React from 'react';
import { useEffect, useState } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import InventoryList from './components/inventoryList/inventory-list';
import InventoryDetailScreen from './components/inventoryList/inventory-detail';
import LoginScreen from './components/login-screen';
import { AuthStore } from './auth/auth-store';
import { ActivityIndicator, View } from 'react-native';
import {MD3LightTheme, PaperProvider} from "react-native-paper";

export type RootStackParamList = {
  Login: undefined;
  InventoryList: undefined;
  InventoryDetail: { id: string };
};

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function App() {
  const [ready, setReady] = useState(false);
  const [hasToken, setHasToken] = useState(false);

  useEffect(() => {
    (async () => {
      // load token from AsyncStorage into memory
      const token = await AuthStore.load();
      setHasToken(!!token);
      setReady(true);
    })();
  }, []);

  if (!ready) {
    return (
        <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center' }}>
          <ActivityIndicator />
        </View>
    );
  }

  return (
      <PaperProvider theme={MD3LightTheme}>
        <NavigationContainer>
          <Stack.Navigator initialRouteName={hasToken ? 'InventoryList' : 'Login'}>
            <Stack.Screen
                name="Login"
                component={LoginScreen}
                options={{ title: 'Sign in' }}
            />
            <Stack.Screen
                name="InventoryList"
                component={InventoryList}
                options={{ title: 'Inventory' }}
            />
            <Stack.Screen
                name="InventoryDetail"
                component={InventoryDetailScreen}
                options={{ title: 'Item' }}
            />
          </Stack.Navigator>
        </NavigationContainer>
      </PaperProvider>
  );
}
