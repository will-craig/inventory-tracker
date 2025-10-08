import React, { useState } from "react";
import { View, Text, Button, Alert, ActivityIndicator } from "react-native";
import { useAuth } from "../auth/auth-provider";

export default function LoginScreen() {
  const { isReady, isSignedIn, signIn } = useAuth();
  const [busy, setBusy] = useState(false);

  const handleLogin = async () => {
    setBusy(true);
    try {
      await signIn();
      // No manual navigation needed: navigator will swap stacks when isSignedIn flips to true
    } catch (e: any) {
      const msg = e?.message ?? "Sign-in failed";
      Alert.alert("Sign-in error", msg);
    } finally {
      setBusy(false);
    }
  };

  if (!isReady) {
    return (
      <View style={{ flex: 1, alignItems: "center", justifyContent: "center" }}>
        <ActivityIndicator />
      </View>
    );
  }

  return (
    <View
      style={{
        flex: 1,
        alignItems: "center",
        justifyContent: "center",
        padding: 24,
        gap: 12,
      }}
    >
      <Text style={{ fontSize: 22, fontWeight: "700" }}>Welcome</Text>
      <Text style={{ opacity: 0.7, marginBottom: 12 }}>
        Sign in to continue
      </Text>

      <Button
        title={busy ? "Signing in…" : "Sign in with Microsoft"}
        onPress={handleLogin}
        disabled={busy || isSignedIn}
      />

      {busy && <ActivityIndicator style={{ marginTop: 12 }} />}
    </View>
  );
}
