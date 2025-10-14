import React from "react";
import { render, screen, fireEvent } from "@testing-library/react-native";
import SettingsScreen from "../../app/(protected)/settings";
import { Provider as PaperProvider } from "react-native-paper";
import { SettingsContext, SettingsContextValue } from "../../providers/settings-context";
import { AuthContext } from "../../providers/auth-context";

function renderSettings(overrides?: Partial<SettingsContextValue>) {
  const settings: SettingsContextValue = {
    settings: { theme: "system", confirmDelete: true },
    setTheme: jest.fn(),
    setConfirmDelete: jest.fn(),
    ...overrides,
  } as any;

  // Mock AuthContext to avoid errors on sign out
  const authValue = { signOut: jest.fn() } as any;

  return {
    ...render(
      <PaperProvider>
        <AuthContext.Provider value={authValue}>
          <SettingsContext.Provider value={settings}>
            <SettingsScreen />
          </SettingsContext.Provider>
        </AuthContext.Provider>
      </PaperProvider>
    ),
    settings,
    authValue,
  };
}

describe("SettingsScreen", () => {
  test("changes theme via list items", () => {
    const { settings } = renderSettings();

    fireEvent.press(screen.getByTestId("theme-light"));
    expect(settings.setTheme).toHaveBeenCalledWith("light");

    fireEvent.press(screen.getByTestId("theme-dark"));
    expect(settings.setTheme).toHaveBeenCalledWith("dark");
  });

  test("toggles confirm before delete", () => {
    const { settings } = renderSettings({ settings: { theme: "system", confirmDelete: true } as any });

    fireEvent.press(screen.getByTestId("confirm-delete-item"));
    expect(settings.setConfirmDelete).toHaveBeenCalledWith(false);
  });
});
