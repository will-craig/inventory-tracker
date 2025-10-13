import { AuthProvider, AuthContext } from "../providers/auth-context";
import {StatusBar} from "expo-status-bar";
import { Stack, useRouter, useSegments } from "expo-router";
import React from "react";
import LoadingScreen from "../components/loading-screen";

function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isReady } = React.useContext(AuthContext);
  const router = useRouter();
  const segments = useSegments();

  React.useEffect(() => {
    if (!isReady) 
        return;

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


export default function RootLayout() {
    return (
        <AuthProvider>
            <AuthGuard>
                <StatusBar style="auto" />
                <Stack>
                    <Stack.Screen name="(protected)" options={{headerShown: false}} />
                    <Stack.Screen name="login" options={{headerShown: false}} />
                </Stack>
            </AuthGuard>
        </AuthProvider>
    );
}