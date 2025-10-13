import { registerRootComponent } from 'expo';
import { http } from '../../services/clients/http';
import {ApiClient} from "../../services/clients/api-client";
import { View, Text } from 'react-native';

//export const api = new ApiClient(http.defaults.baseURL!, http);

export default function LandingPage() {
  return (
    <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
      <Text style={{ fontSize: 24, fontWeight: "bold" }}>Welcome to Smart Track!</Text>
      <Text style={{ marginTop: 12 }}>You are now logged in.</Text>
    </View>
  );
}