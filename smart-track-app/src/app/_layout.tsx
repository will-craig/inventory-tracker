import { AuthProvider, AuthContext } from "../providers/auth-context";
import { StatusBar } from "expo-status-bar";
import { Stack, useRouter, useSegments } from "expo-router";
import React from "react";
import LoadingScreen from "../components/loading-screen";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as PaperProvider, MD3DarkTheme, MD3LightTheme, adaptNavigationTheme } from "react-native-paper";
import { useColorScheme, View } from "react-native";
import { SettingsProvider, SettingsContext } from "../providers/settings-context";
import {
  DefaultTheme as NavigationDefaultTheme,
  DarkTheme as NavigationDarkTheme,
  ThemeProvider as NavigationThemeProvider,
} from "@react-navigation/native";

const queryClient = new QueryClient();

function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isReady } = React.useContext(AuthContext);
  const router = useRouter();
  const segments = useSegments();

  React.useEffect(() => {
    if (!isReady) return;

    const inLogin = segments[0] === "login";

    if (!isAuthenticated && !inLogin) {
      router.replace("/login");
    } else if (isAuthenticated && inLogin) {
      router.replace("/(protected)");
    }
  }, [isAuthenticated, isReady, segments, router]);

  if (!isReady) return <LoadingScreen message="Checking your session…" />;
  return <>{children}</>;
}

function ThemedApp() {
  const { settings } = React.useContext(SettingsContext);
  const scheme = useColorScheme();
  const useDark = settings.theme === "dark" || (settings.theme === "system" && scheme === "dark");
  const paperTheme = useDark ? MD3DarkTheme : MD3LightTheme;

  // Create a matching React Navigation theme adapted from Paper
  const { LightTheme: AdaptedNavLight, DarkTheme: AdaptedNavDark } = adaptNavigationTheme({
    reactNavigationLight: NavigationDefaultTheme,
    reactNavigationDark: NavigationDarkTheme,
  });
  const navTheme = useDark ? AdaptedNavDark : AdaptedNavLight;

  return (
    <PaperProvider theme={paperTheme}>
      <QueryClientProvider client={queryClient}>
        <NavigationThemeProvider value={navTheme}>
          <StatusBar style={useDark ? "light" : "dark"} backgroundColor={paperTheme.colors.background} />
          <View style={{ flex: 1, backgroundColor: paperTheme.colors.background }}>
            <Stack
              screenOptions={{
                contentStyle: { backgroundColor: paperTheme.colors.background },
              }}
            >
              <Stack.Screen name="(protected)" options={{ headerShown: false }} />
              <Stack.Screen name="login" options={{ headerShown: false }} />
            </Stack>
          </View>
        </NavigationThemeProvider>
      </QueryClientProvider>
    </PaperProvider>
  );
}

export default function RootLayout() {
  return (
    <AuthProvider>
      <AuthGuard>
        <SettingsProvider>
          <ThemedApp />
        </SettingsProvider>
      </AuthGuard>
    </AuthProvider>
  );
}
