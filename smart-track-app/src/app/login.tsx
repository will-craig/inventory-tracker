import React, { useContext } from "react";
import { View, Text, Button, Alert } from "react-native";
import { AuthContext } from "../providers/auth-context";

export default function Login() {
  const authContext = useContext(AuthContext);
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
      <Button title="Sign In" onPress={authContext.signIn} />
    </View>
  );
}
