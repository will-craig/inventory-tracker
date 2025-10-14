import React from "react";
import { View } from "react-native";
import { useRouter } from "expo-router";
import { useQuery } from "@tanstack/react-query";
import { InventoryService } from "../../../services/inventory-service";
import { List, ActivityIndicator, FAB, Text } from "react-native-paper";
import type { InventoryItem } from "../../../domain/models/inventory-item";
import { formatQty } from "../../../domain/units";

export default function InventoryListScreen() {
  const router = useRouter();
  const {
    data: items,
    isLoading,
    error,
  } = useQuery<InventoryItem[]>({
    queryKey: ["inventory"],
    queryFn: () => InventoryService.list(),
  });

  if (isLoading) {
    return (
      <View style={{ flex: 1, justifyContent: "center", alignItems: "center" }}>
        <ActivityIndicator />
        <Text style={{ marginTop: 12 }}>Loading inventory…</Text>
      </View>
    );
  }

  if (error) {
    return (
      <View
        style={{
          flex: 1,
          justifyContent: "center",
          alignItems: "center",
          padding: 24,
        }}
      >
        <Text>Failed to load inventory.</Text>
        <Text style={{ opacity: 0.7, marginTop: 8 }}>{String(error)}</Text>
      </View>
    );
  }

  return (
    <View style={{ flex: 1 }}>
      <List.Section>
        <List.Subheader>Inventory</List.Subheader>
        {(items ?? []).map((item) => (
          <List.Item
            key={item.id ?? item.name}
            title={item.name}
            description={formatQty(item.quantity, item.unit)}
            onPress={() => router.push(`/(protected)/inventory/${item.id}`)}
            right={(props) => <List.Icon {...props} icon="chevron-right" />}
          />
        ))}
        {(items ?? []).length === 0 && (
          <View style={{ padding: 24 }}>
            <Text>No items yet.</Text>
          </View>
        )}
      </List.Section>

      <FAB
        icon="plus"
        style={{ position: "absolute", right: 16, bottom: 16 }}
        onPress={() => router.push("/(protected)/inventory/new")}
        label="Add"
      />
    </View>
  );
}
