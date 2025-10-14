import { useContext } from "react";
import { AuthContext } from "../../providers/auth-context";
import { Link, Stack, useRouter } from "expo-router";
import { View, Button } from "react-native";
import { Text, TouchableOpacity } from "react-native";
import { useTheme } from "react-native-paper";

export default function ProtectedLayout() {
  const authState = useContext(AuthContext);
  const router = useRouter();
  const theme = useTheme();

  function HeaderSettingsButton() {
    return (
      <Link href="/(protected)/settings" asChild>
        <TouchableOpacity style={{ paddingHorizontal: 12 }}>
          <Text style={{ color: theme.colors.primary }}>Settings</Text>
        </TouchableOpacity>
      </Link>
    );
  }

  return (
    <Stack
      screenOptions={{
        headerRight: () => <HeaderSettingsButton />, // persistent shortcut
        headerTitle: "Smart Track",
        headerStyle: { backgroundColor: theme.colors.elevation.level2 },
        headerTitleStyle: { color: theme.colors.onSurface },
        headerTintColor: theme.colors.onSurface,
        contentStyle: { backgroundColor: theme.colors.background },
      }}
    >
      {/* You can still add per-screen options below if you want */}
      <Stack.Screen name="index" options={{ title: "Home" }} />
      <Stack.Screen name="settings" options={{ title: "Settings" }} />
    </Stack>
  );
}
