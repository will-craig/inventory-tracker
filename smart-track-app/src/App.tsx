// App.tsx
import * as React from "react";
import { NavigationContainer } from "@react-navigation/native";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import InventoryList from "./components/inventoryList/inventory-list";
import InventoryDetailScreen from "./components/inventoryList/inventory-detail";
import LoginScreen from "./components/login-screen";

export type RootStackParamList = {
  Login: undefined;
  InventoryList: undefined;
  InventoryDetail: { id: string };
};

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function App() {
  return (
    <NavigationContainer>
      <Stack.Navigator initialRouteName="Login">
        <Stack.Screen
          name="Login"
          component={LoginScreen}
          options={{ title: "Sign in" }}
        />
        <Stack.Screen
          name="InventoryList"
          component={InventoryList}
          options={{ title: "Inventory" }}
        />
        <Stack.Screen
          name="InventoryDetail"
          component={InventoryDetailScreen}
          options={{ title: "Item" }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
