import * as React from "react";
import { View, Button, Alert } from "react-native";
import { AuthContext } from "../../providers/auth-context";

export default function SettingsScreen() {
  const { signOut } = React.useContext(AuthContext);

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

  return (
    <View style={{ flex: 1, padding: 16, gap: 12 }}>
      {/* ... other settings rows ... */}
      <Button title="Sign out" onPress={handleSignOut} />
    </View>
  );
}
