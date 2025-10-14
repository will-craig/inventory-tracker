import { useContext } from "react";
import { AuthContext } from "../../providers/auth-context";
import { Link, Stack, useRouter } from "expo-router";
import { View, Button } from "react-native";
import { Text, TouchableOpacity } from "react-native";

export default function ProtectedLayout() {
  const authState = useContext(AuthContext);
  const router = useRouter();

  function HeaderSettingsButton() {
    return (
      <Link href="/(protected)/settings" asChild>
        <TouchableOpacity style={{ paddingHorizontal: 12 }}>
          <Text>Settings</Text>
        </TouchableOpacity>
      </Link>
    );
  }

  return (
    <Stack
      screenOptions={{
        headerRight: () => <HeaderSettingsButton />, // persistent shortcut
        headerTitle: "Smart Track",
      }}
    >
      {/* You can still add per-screen options below if you want */}
      <Stack.Screen name="index" options={{ title: "Home" }} />
      <Stack.Screen name="settings" options={{ title: "Settings" }} />
    </Stack>
  );
}
