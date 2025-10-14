import React, { useState } from "react";
import { View, Alert } from "react-native";
import { useRouter } from "expo-router";
import { InventoryService } from "../../../services/inventory-service";
import type { InventoryItem } from "../../../domain/models/inventory-item";
import { allUnits, unitLabel } from "../../../domain/units";
import { TextInput, Button, Text } from "react-native-paper";
import { Picker } from "@react-native-picker/picker";

export default function InventoryCreateScreen() {
  const router = useRouter();
  const [item, setItem] = useState<InventoryItem>({
    name: "",
    quantity: 0,
    unit: (allUnits[0] as any) ?? "None",
  });
  const [saving, setSaving] = useState(false);

  const onSave = async () => {
    if (!item.name?.trim()) {
      Alert.alert("Validation", "Name is required");
      return;
    }
    setSaving(true);
    try {
      await InventoryService.save(item);
      router.replace("/(protected)/inventory");
    } catch (e) {
      Alert.alert("Error", "Failed to create item");
    } finally {
      setSaving(false);
    }
  };

  return (
    <View style={{ flex: 1, padding: 16, gap: 12 }}>
      <TextInput
        label="Name"
        value={item.name}
        onChangeText={(t) => setItem({ ...item, name: t })}
      />

      <TextInput
        label="Quantity"
        keyboardType="numeric"
        value={String(item.quantity ?? "")}
        onChangeText={(t) => setItem({ ...item, quantity: Number(t) || 0 })}
      />

      <View>
        <Text style={{ marginBottom: 6 }}>Unit</Text>
        <Picker
          selectedValue={item.unit}
          onValueChange={(val) => setItem({ ...item, unit: val })}
        >
          {allUnits.map((u) => (
            <Picker.Item key={String(u)} label={unitLabel(u)} value={u} />
          ))}
        </Picker>
      </View>

      <TextInput
        label="Expiry (YYYY-MM-DD)"
        placeholder="Optional"
        value={
          item.expiryDate ? item.expiryDate.toISOString().slice(0, 10) : ""
        }
        onChangeText={(t) =>
          setItem({ ...item, expiryDate: t ? new Date(t) : undefined })
        }
      />

      <View style={{ flexDirection: "row", gap: 12, marginTop: 16 }}>
        <Button mode="contained" loading={saving} onPress={onSave}>
          Save
        </Button>
        <Button mode="outlined" onPress={() => router.back()} disabled={saving}>
          Cancel
        </Button>
      </View>
    </View>
  );
}
