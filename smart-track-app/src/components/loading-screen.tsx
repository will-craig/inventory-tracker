// components/LoadingScreen.tsx
import * as React from "react";
import { View, ActivityIndicator, Text } from "react-native";

export default function LoadingScreen({ message = "Loading…" }: { message?: string }) {
  return (
    <View style={{ flex: 1, justifyContent: "center", alignItems: "center", gap: 12 }}>
      <ActivityIndicator size="large" />
      <Text>{message}</Text>
    </View>
  );
}
