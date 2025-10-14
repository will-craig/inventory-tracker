import React, { createContext, useCallback, useEffect, useMemo, useState } from "react";
import AsyncStorage from "@react-native-async-storage/async-storage";

export type ThemeSetting = "system" | "light" | "dark";

export type SettingsState = {
  theme: ThemeSetting;
  confirmDelete: boolean;
};

export type SettingsContextValue = {
  settings: SettingsState;
  setTheme: (theme: ThemeSetting) => void;
  setConfirmDelete: (value: boolean) => void;
};

const DEFAULT_SETTINGS: SettingsState = {
  theme: "system",
  confirmDelete: true,
};

const STORAGE_KEY = "user_settings_v1";

export const SettingsContext = createContext<SettingsContextValue>({
  settings: DEFAULT_SETTINGS,
  setTheme: () => {},
  setConfirmDelete: () => {},
});

export function SettingsProvider({ children }: { children: React.ReactNode }) {
  const [settings, setSettings] = useState<SettingsState>(DEFAULT_SETTINGS);

  useEffect(() => {
    (async () => {
      try {
        const raw = await AsyncStorage.getItem(STORAGE_KEY);
        if (raw) {
          const parsed = JSON.parse(raw) as Partial<SettingsState>;
          setSettings((prev) => ({ ...prev, ...parsed }));
        }
      } catch {
        // ignore
      }
    })();
  }, []);

  const persist = useCallback(async (next: SettingsState) => {
    try {
      await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    } catch {
      // ignore persistence errors for now
    }
  }, []);

  const setTheme = useCallback(
    (theme: ThemeSetting) => {
      setSettings((prev) => {
        const next = { ...prev, theme };
        void persist(next);
        return next;
      });
    },
    [persist]
  );

  const setConfirmDelete = useCallback(
    (confirmDelete: boolean) => {
      setSettings((prev) => {
        const next = { ...prev, confirmDelete };
        void persist(next);
        return next;
      });
    },
    [persist]
  );

  const value = useMemo<SettingsContextValue>(
    () => ({ settings, setTheme, setConfirmDelete }),
    [settings, setTheme, setConfirmDelete]
  );

  return <SettingsContext.Provider value={value}>{children}</SettingsContext.Provider>;
}
