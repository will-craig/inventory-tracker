// components/inventoryList/inventory-form.tsx
import React, { useEffect, useMemo, useRef, useState } from "react";
import { Keyboard, Platform, View } from "react-native";
import {
  Button,
  HelperText,
  Menu,
  Text,
  TextInput,
  useTheme,
} from "react-native-paper";
import type { InventoryItem } from "../../domain/models/inventory-item";
import { Unit } from "../../services/clients/api-client";
import { allUnits } from "../../domain/units";

type Props = {
  initial?: InventoryItem;
  onCancel: () => void;
  onSubmit: (values: Partial<InventoryItem>) => Promise<void> | void;
};

export default function InventoryForm({ initial, onCancel, onSubmit }: Props) {
  const theme = useTheme();

  // Form state (strings to keep typing easy)
  const [name, setName] = useState("");
  const [quantity, setQuantity] = useState("1");
  const [unit, setUnit] = useState<Unit>(Unit.Part);
  const [expiryDate, setExpiryDate] = useState<string>("");

  // UI state
  const [busy, setBusy] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  // refs for Return/Enter focusing
  const qtyRef = useRef<any>(null);
  const dateRef = useRef<any>(null);

  // init / hydrate
  useEffect(() => {
    setName(initial?.name ?? "");
    setQuantity(
      initial?.quantity != null && !Number.isNaN(initial.quantity)
        ? String(initial.quantity)
        : "1",
    );
    setUnit(initial?.unit ?? Unit.Part);

    const d = initial?.expiryDate
      ? new Date(initial.expiryDate).toISOString().slice(0, 10)
      : "";
    setExpiryDate(d);
  }, [initial]);

  // tiny validation
  const errors = useMemo(() => {
    const e: Record<string, string> = {};
    if (!name.trim()) e.name = "Name is required";
    if (quantity && !/^\d+$/.test(quantity))
      e.quantity = "Enter a whole number";
    if (expiryDate && !/^\d{4}-\d{2}-\d{2}$/.test(expiryDate))
      e.expiryDate = "Use format YYYY-MM-DD";
    return e;
  }, [name, quantity, expiryDate]);

  const handleSubmit = async (addAnother: boolean = false) => {
    // quick guard
    if (Object.keys(errors).length) return;

    setBusy(true);
    Keyboard.dismiss();

    const payload: Partial<InventoryItem> = {
      id: initial?.id, // present when editing
      name: name.trim(),
      quantity: quantity ? parseInt(quantity, 10) : undefined,
      unit: unit as any, // your toApi() will map this to the enum (default "Part")
      expiryDate: expiryDate ? new Date(expiryDate + "T00:00:00Z") : undefined,
    };

    try {
      await onSubmit(payload);
      if (addAnother) {
        // reset fast for the next item
        setName("");
        setQuantity("1");
        setUnit(Unit.Part);
        setExpiryDate("");
        // focus name for speed
        setTimeout(() => {
          (nameRef.current as any)?.focus?.();
        }, 50);
      }
    } finally {
      setBusy(false);
    }
  };

  const nameRef = useRef<any>(null);

  return (
    <View style={{ padding: 16, gap: 12 }}>
      <Text variant="titleLarge">{initial?.id ? "Edit item" : "Add item"}</Text>

      <TextInput
        ref={nameRef}
        label="Name"
        value={name}
        onChangeText={setName}
        autoCapitalize="sentences"
        returnKeyType="next"
        onSubmitEditing={() => qtyRef.current?.focus?.()}
        error={!!errors.name}
      />
      <HelperText type="error" visible={!!errors.name}>
        {errors.name}
      </HelperText>

      <TextInput
        ref={qtyRef}
        label="Quantity"
        value={quantity}
        onChangeText={setQuantity}
        keyboardType={Platform.select({
          ios: "number-pad",
          android: "numeric",
          default: "numeric",
        })}
        returnKeyType="next"
        onSubmitEditing={() => dateRef.current?.focus?.()}
        error={!!errors.quantity}
      />
      <HelperText type="error" visible={!!errors.quantity}>
        {errors.quantity}
      </HelperText>

      {/* Unit dropdown */}
      <View style={{ flexDirection: "row", alignItems: "center", gap: 8 }}>
        <Menu
          visible={menuOpen}
          onDismiss={() => setMenuOpen(false)}
          anchor={
            <Button
              mode="outlined"
              onPress={() => setMenuOpen(true)}
              style={{ flexGrow: 1 }}
            >
              Unit: {unit}
            </Button>
          }
        >
          {allUnits.map((opt) => (
            <Menu.Item
              key={opt}
              title={opt}
              onPress={() => {
                setUnit(opt);
                setMenuOpen(false);
              }}
            />
          ))}
        </Menu>
      </View>

      <TextInput
        ref={dateRef}
        label="Expiry (YYYY-MM-DD)"
        value={expiryDate}
        onChangeText={setExpiryDate}
        placeholder="YYYY-MM-DD (optional)"
        returnKeyType="done"
        error={!!errors.expiryDate}
      />
      <HelperText type="error" visible={!!errors.expiryDate}>
        {errors.expiryDate}
      </HelperText>

      {/* Actions */}
      <View
        style={{
          flexDirection: "row",
          justifyContent: "flex-end",
          gap: 12,
          marginTop: 4,
        }}
      >
        <Button mode="text" onPress={onCancel} disabled={busy}>
          Cancel
        </Button>
        <Button
          mode="contained"
          onPress={() => handleSubmit(false)}
          loading={busy}
          disabled={
            busy || !!errors.name || !!errors.quantity || !!errors.expiryDate
          }
        >
          Save
        </Button>
        {!initial?.id && (
          <Button
            mode="contained-tonal"
            onPress={() => handleSubmit(true)}
            disabled={
              busy || !!errors.name || !!errors.quantity || !!errors.expiryDate
            }
          >
            Save & add another
          </Button>
        )}
      </View>
    </View>
  );
}
