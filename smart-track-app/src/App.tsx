import React, { useState } from 'react';
import { View, Text, Button, Alert, ActivityIndicator } from 'react-native';
import { signIn } from './services/auth-service';

export default function LoginScreen({ onSignedIn }: { onSignedIn: () => void }) {
  const [busy, setBusy] = useState(false);

  const handleLogin = async () => {
    setBusy(true);
    try {
      await signIn();
      onSignedIn();
    } catch (e: any) {
      const msg = e?.message ?? 'Sign-in failed';
      Alert.alert('Sign-in error', msg);
    } finally {
      setBusy(false);
    }
  };

  return (
    <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24, gap: 12 }}>
      <Text style={{ fontSize: 22, fontWeight: '700' }}>Welcome</Text>
      <Text style={{ opacity: 0.7, marginBottom: 12 }}>Sign in to continue</Text>

      <Button title={busy ? 'Signing in…' : 'Sign in with Microsoft'} onPress={handleLogin} disabled={busy} />

      {busy && <ActivityIndicator style={{ marginTop: 12 }} />}
    </View>
  );
}
