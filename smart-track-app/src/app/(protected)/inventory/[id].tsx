import React, { useEffect, useState } from "react";
import { View, Alert, Platform } from "react-native";
import { useLocalSearchParams, useRouter } from "expo-router";
import { InventoryService } from "../../../services/inventory-service";
import type { InventoryItem } from "../../../domain/models/inventory-item";
import { allUnits, unitLabel } from "../../../domain/units";
import { TextInput, Button, Text, useTheme } from "react-native-paper";
import { Picker } from "@react-native-picker/picker";
import DateTimePicker, {
  DateTimePickerEvent,
} from "@react-native-community/datetimepicker";
import { SettingsContext } from "../../providers/settings-context";

export default function InventoryDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const settingsCtx = React.useContext(SettingsContext);
  const theme = useTheme();
  const [item, setItem] = useState<InventoryItem | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showDate, setShowDate] = useState(false);

  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        const data = await InventoryService.get(id!);
        if (mounted) setItem(data);
      } catch (e) {
        Alert.alert("Error", "Failed to load item");
      } finally {
        if (mounted) setLoading(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [id]);

  const onSave = async () => {
    if (!item) return;
    if (!item.name?.trim()) {
      Alert.alert("Validation", "Name is required");
      return;
    }
    setSaving(true);
    try {
      await InventoryService.save(item);
      router.back();
    } catch (e) {
      Alert.alert("Error", "Failed to save item");
    } finally {
      setSaving(false);
    }
  };

  const onDelete = async () => {
    if (!item?.id) return;

    const performDelete = async () => {
      try {
        await InventoryService.delete(item.id!);
        router.back();
      } catch (e) {
        Alert.alert("Error", "Failed to delete item");
      }
    };

    if (settingsCtx.settings.confirmDelete) {
      Alert.alert("Delete Item", "Are you sure you want to delete this item?", [
        { text: "Cancel", style: "cancel" },
        { text: "Delete", style: "destructive", onPress: performDelete },
      ]);
    } else {
      await performDelete();
    }
  };

  if (loading || !item) {
    return (
      <View
        style={{
          flex: 1,
          alignItems: "center",
          justifyContent: "center",
          backgroundColor: theme.colors.background,
        }}
      >
        <Text>Loading…</Text>
      </View>
    );
  }

  const onDateChange = (_: DateTimePickerEvent, date?: Date) => {
    setShowDate(false);
    if (date) setItem({ ...item!, expiryDate: date });
  };

  return (
    <View style={{ flex: 1, padding: 16, gap: 12, backgroundColor: theme.colors.background }}>
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

      {/* Fast native date picker: compact on iOS, calendar on Android; fallback to text on web */}
      {Platform.OS === "web" ? (
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
      ) : (
        <View>
          <Text style={{ marginBottom: 6 }}>Expiry</Text>
          <Button mode="outlined" onPress={() => setShowDate(true)}>
            {item.expiryDate ? item.expiryDate.toDateString() : "Set date"}
          </Button>
          {showDate && (
            <DateTimePicker
              value={item.expiryDate ?? new Date()}
              mode="date"
              display={Platform.OS === "ios" ? "compact" : "calendar"}
              onChange={onDateChange}
            />
          )}
        </View>
      )}

      <View style={{ flexDirection: "row", gap: 12, marginTop: 16 }}>
        <Button testID="save-button" mode="contained" loading={saving} onPress={onSave}>
          Save
        </Button>
        <Button mode="outlined" onPress={() => router.back()} disabled={saving}>
          Cancel
        </Button>
        <View style={{ flex: 1 }} />
        <Button
          testID="delete-button"
          mode="contained-tonal"
          onPress={onDelete}
          disabled={!item.id || saving}
        >
          Delete
        </Button>
      </View>
    </View>
  );
}
