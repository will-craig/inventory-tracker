import * as React from "react";
import { View, Alert, useColorScheme } from "react-native";
import { AuthContext } from "../../providers/auth-context";
import { SettingsContext, ThemeSetting } from "../../providers/settings-context";
import { List, RadioButton, Switch, Divider, Button, useTheme } from "react-native-paper";

export default function SettingsScreen() {
  const { signOut } = React.useContext(AuthContext);
  const { settings, setTheme, setConfirmDelete } = React.useContext(SettingsContext);
  const scheme = useColorScheme();
  const theme = useTheme();

  const handleSignOut = () => {
    Alert.alert("Sign out", "Are you sure you want to sign out?", [
      { text: "Cancel", style: "cancel" },
      {
        text: "Sign out",
        style: "destructive",
        onPress: async () => {
          await signOut();
        },
      },
    ]);
  };

  const onThemeChange = (value: ThemeSetting) => setTheme(value);

  return (
    <View style={{ flex: 1, backgroundColor: theme.colors.background }}>
      <List.Section>
        <List.Subheader>Appearance</List.Subheader>
        <RadioButton.Group onValueChange={(v) => onThemeChange(v as ThemeSetting)} value={settings.theme}>
          <List.Item
            testID="theme-system"
            title="System"
            description={`Use device theme (currently ${scheme ?? "unknown"})`}
            left={(props) => <List.Icon {...props} icon="theme-light-dark" />}
            right={() => <RadioButton value="system" />}
            onPress={() => onThemeChange("system")}
          />
          <List.Item
            testID="theme-light"
            title="Light"
            left={(props) => <List.Icon {...props} icon="weather-sunny" />}
            right={() => <RadioButton value="light" />}
            onPress={() => onThemeChange("light")}
          />
          <List.Item
            testID="theme-dark"
            title="Dark"
            left={(props) => <List.Icon {...props} icon="weather-night" />}
            right={() => <RadioButton value="dark" />}
            onPress={() => onThemeChange("dark")}
          />
        </RadioButton.Group>
      </List.Section>

      <Divider />

      <List.Section>
        <List.Subheader>Behavior</List.Subheader>
        <List.Item
          testID="confirm-delete-item"
          title="Confirm before delete"
          description="Ask for confirmation before deleting an item"
          left={(props) => <List.Icon {...props} icon="alert-circle-outline" />}
          right={() => (
            <Switch
              testID="confirm-delete-switch"
              value={settings.confirmDelete}
              onValueChange={setConfirmDelete}
            />
          )}
          onPress={() => setConfirmDelete(!settings.confirmDelete)}
        />
      </List.Section>

      <Divider />

      <View style={{ padding: 16, gap: 12 }}>
        <Button mode="contained-tonal" onPress={handleSignOut}>
          Sign out
        </Button>
      </View>
    </View>
  );
}
