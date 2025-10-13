import React, { useState } from "react";
import { View, Text, Button, Alert, ActivityIndicator } from "react-native";

export default function LoginScreen() {
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
    </View>
  );
}
