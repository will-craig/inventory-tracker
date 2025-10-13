import { createContext, PropsWithChildren, useEffect, useState } from 'react';
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SplashScreen } from 'expo-router';

SplashScreen.preventAutoHideAsync();

type AuthState = {
  //accessToken: string | null;
  isAuthenticated: boolean;
  isReady: boolean;
  signIn: () => Promise<void>;
  signOut: () => Promise<void>;
};

const authStorageKey = 'auth-key';

export const AuthContext = createContext<AuthState>({
    //accessToken: null,
    isAuthenticated: false, 
    isReady: false,
    signIn: async () => {},
    signOut: async () => {}
});

export function AuthProvider({children}: PropsWithChildren) {
    const [isReady, setIsReady] = useState(false);
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    const storeAuthState = async (isLoggedIn: boolean) => {
        try {
            const value = JSON.stringify({ isAuthenticated: isLoggedIn });
            await AsyncStorage.setItem(authStorageKey, value);
        } catch (error) {
            console.error('Failed to store auth state', error);
        }
    };

    const signIn = async () => {
        // Simulate waiting for a real authentication
        await new Promise((resolve) => setTimeout(resolve, 1000));
        console.log("Signing in...");
        setIsAuthenticated(true);
        await storeAuthState(true);
    };

    const signOut = async () => {
        console.log("Signing out...");
        setIsAuthenticated(false);
        await storeAuthState(false);
    };

    useEffect(() => {
        const loadAuthState = async () => {
            try {
                const storedValue = await AsyncStorage.getItem(authStorageKey);
                if (storedValue) {
                    const parsed = JSON.parse(storedValue);
                    setIsAuthenticated(parsed.isAuthenticated);
                }
            } catch (error) {
                console.error('Failed to load auth state', error);
            }
            setIsReady(true);
        };
        loadAuthState();
    }, []);

    useEffect(() => {
        if (isReady) {
        SplashScreen.hideAsync();
        }
    }, [isReady]);

    return (
        <AuthContext.Provider value={{isAuthenticated, isReady, signIn, signOut}}>
            {children}
        </AuthContext.Provider>
    );
}