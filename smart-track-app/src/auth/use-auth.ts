import { useEffect, useState, useCallback } from 'react';
import { AuthStore } from './auth-store';
import { AuthService } from '../services/auth.services';

export function UseAuth() {
    const [token, setToken] = useState<string | null>(AuthStore.get());

    useEffect(() => {
        // re-check after hydration (in case App.tsx loads slower)
        AuthStore.load().then(t => setToken(t));
    }, []);

    const login = useCallback(async (u: string, p: string) => {
        await AuthService.login(u, p);
        setToken(AuthStore.get());
    }, []);

    const logout = useCallback(async () => {
        await AuthService.logout();
        setToken(null);
    }, []);

    const isAuthed = !!token;

    return { token, isAuthed, login, logout };
}
