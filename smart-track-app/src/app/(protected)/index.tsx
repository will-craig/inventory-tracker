import { View, Text } from "react-native";
import { Link } from "expo-router";
import { Button } from "react-native-paper";

export default function LandingPage() {
  return (
    <View
      style={{
        flex: 1,
        alignItems: "center",
        justifyContent: "center",
        padding: 24,
        gap: 16,
      }}
    >
      <Text style={{ fontSize: 24, fontWeight: "bold" }}>
        Welcome to Smart Track!
      </Text>
      <Text style={{ marginTop: 8, opacity: 0.8, textAlign: "center" }}>
        Manage your inventory with ease.
      </Text>
      <Link href="/(protected)/inventory" asChild>
        <Button mode="contained" style={{ marginTop: 16 }}>
          View Inventory
        </Button>
      </Link>
    </View>
  );
}
