import { useRouter } from "expo-router";
import {createContext, PropsWithChildren, useEffect, useState} from 'react';
import AsyncStorage from "@react-native-async-storage/async-storage";

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

export function AuthProvider({children} : PropsWithChildren) {
    const [isReady, setIsReady] = useState(false);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const router = useRouter();
    
    const storeAuthState = async (isLoggedIn: boolean) => {
        try {
            const value = JSON.stringify({ isAuthenticated: isLoggedIn });
            await AsyncStorage.setItem(authStorageKey, value);
            
        } catch (error) {
            console.error('Failed to store auth state', error);
        }
    }
    
    const signIn = async () => {
        // perform sign-in logic here, e.g. call an API, get a token, etc.
        setIsAuthenticated(true);
        await storeAuthState(true);
    }
    const signOut = async () => {
        // perform sign-out logic here, e.g. clear token, call an API, etc.
        setIsAuthenticated(false);
        await storeAuthState(false);
        
    }
    
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
    
    return (
        <AuthContext.Provider value={{isAuthenticated, isReady, signIn, signOut}}>
            {children}
        </AuthContext.Provider>
    )
}