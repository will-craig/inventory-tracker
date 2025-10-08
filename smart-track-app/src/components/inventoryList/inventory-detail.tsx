// src/screens/StockDetailScreen.tsx
import React, { useEffect, useState } from "react";
import { View, Text, ActivityIndicator } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { RootStackParamList } from "../../App";
import { InventoryItem } from "../../domain/models/inventory-item";
import { InventoryService } from "../../services/inventory-service";
import { formatQty } from "../../domain/units";

type Props = NativeStackScreenProps<RootStackParamList, "InventoryDetail">;

export default function InventoryDetailScreen({ route }: Props) {
  const { id } = route.params;
  const [loading, setLoading] = useState(true);
  const [items, setItem] = useState<InventoryItem>();
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const res = await InventoryService.get(id);
        setItem(res);
      } catch (e: any) {
        setError(e?.message ?? "Failed to load");
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  if (loading) return <ActivityIndicator style={{ marginTop: 40 }} />;
  if (error)
    return <Text style={{ color: "crimson", padding: 16 }}>{error}</Text>;

  // Your API returns an array; show first or a history list
  const current = items;
  if (!current) return <Text style={{ padding: 16 }}>No data</Text>;

  return (
    <View style={{ padding: 16 }}>
      <Text style={{ fontSize: 18, fontWeight: "700" }}>{current.name}</Text>
      <Text style={{ marginTop: 8 }}>
        {formatQty(current.quantity, current.unit)}
      </Text>
      <Text style={{ marginTop: 8 }}>
        Expiry: {current.expiryDate ? current.expiryDate.toDateString() : "-"}
      </Text>
    </View>
  );
}
