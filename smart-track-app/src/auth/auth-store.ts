// src/auth/auth-store.ts
import AsyncStorage from '@react-native-async-storage/async-storage';

const KEY = 'auth.token';

let inMemoryToken: string | null = null;

export const AuthStore = {
    async load(): Promise<string | null> {
        inMemoryToken = await AsyncStorage.getItem(KEY);
        return inMemoryToken;
    },

    get(): string | null {
        return inMemoryToken;
    },
    
    async set(token: string): Promise<void> {
        inMemoryToken = token;
        await AsyncStorage.setItem(KEY, token);
    },

    async clear(): Promise<void> {
        inMemoryToken = null;
        await AsyncStorage.removeItem(KEY);
    },
};
