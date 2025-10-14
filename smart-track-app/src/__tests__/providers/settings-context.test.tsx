import React from "react";
import { render, screen, fireEvent } from "@testing-library/react-native";
import { SettingsProvider, SettingsContext } from "../../providers/settings-context";
import { Text, TouchableOpacity, View } from "react-native";

function Reader() {
  const { settings, setTheme, setConfirmDelete } = React.useContext(SettingsContext);
  return (
    <View>
      <Text testID="theme">{settings.theme}</Text>
      <Text testID="confirmDelete">{String(settings.confirmDelete)}</Text>
      <TouchableOpacity testID="setDark" onPress={() => setTheme("dark")} />
      <TouchableOpacity testID="setConfirmFalse" onPress={() => setConfirmDelete(false)} />
    </View>
  );
}

test("settings default and updates", async () => {
  render(
    <SettingsProvider>
      <Reader />
    </SettingsProvider>
  );

  expect(screen.getByTestId("theme").props.children).toBe("system");
  expect(screen.getByTestId("confirmDelete").props.children).toBe("true");

  fireEvent.press(screen.getByTestId("setDark"));
  expect(screen.getByTestId("theme").props.children).toBe("dark");

  fireEvent.press(screen.getByTestId("setConfirmFalse"));
  expect(screen.getByTestId("confirmDelete").props.children).toBe("false");
});
