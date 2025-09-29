// src/screens/LoginScreen.tsx
import React, { useState } from "react";
import { View, Text, TextInput, Button, Alert } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { RootStackParamList } from "../App";
import { UseAuth } from "../auth/use-auth";

type Props = NativeStackScreenProps<RootStackParamList, "Login">;

export default function LoginScreen({ navigation }: Props) {
  const { login } = UseAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);

    const onLogin = async () => {
        setBusy(true);
        try {
            await login(username, password);
            navigation.replace('InventoryList');
        } catch (e: any) {
            Alert.alert('Login failed', e?.message ?? 'Error');
        } finally {
            setBusy(false);
        }
    };

  return (
    <View style={{ padding: 16, gap: 12 }}>
      <Text style={{ fontSize: 18, fontWeight: "600" }}>Welcome</Text>
      <TextInput
        placeholder="Username"
        autoCapitalize="none"
        value={username}
        onChangeText={setUsername}
      />
      <TextInput
        placeholder="Password"
        secureTextEntry
        value={password}
        onChangeText={setPassword}
      />
      <Button
        title={busy ? "Signing in…" : "Sign in"}
        onPress={onLogin}
        disabled={busy}
      />
    </View>
  );
}
