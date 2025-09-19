import React, { useState } from "react";
import { View, Text, TextInput, Button } from "react-native";
import { Picker } from "@react-native-picker/picker";
import { InventoryItem } from "../../domain/models/inventory-item";
import { Unit } from "../../services/clients/api-client";
import { allUnits, unitLabel } from "../../domain/units";

type Values = Partial<
  Pick<InventoryItem, "name" | "quantity" | "unit" | "expiryDate">
>;

export default function InventoryForm({
  initial,
  onSubmit,
  onCancel,
}: {
  initial?: InventoryItem;
  onSubmit: (values: Values) => Promise<void> | void;
  onCancel: () => void;
}) {
  const [name, setName] = useState(initial?.name ?? "");
  const [quantity, setQuantity] = useState(String(initial?.quantity ?? ""));
  const [unit, setUnit] = useState<Unit | undefined>(
    (initial?.unit && (Unit as any)[initial.unit]
      ? (Unit as any)[initial.unit]
      : Unit.None) as Unit,
  );

  return (
    <View style={{ padding: 16, gap: 12, flex: 1 }}>
      <Text style={{ fontSize: 18, fontWeight: "700" }}>
        {initial ? "Edit item" : "New item"}
      </Text>

      <Text>Name</Text>
      <TextInput value={name} onChangeText={setName} placeholder="Milk..?" />

      <Text>Quantity</Text>
      <TextInput
        value={quantity}
        onChangeText={setQuantity}
        keyboardType="numeric"
      />

      <Text>Unit</Text>
      <Picker selectedValue={unit} onValueChange={(v: Unit) => setUnit(v)}>
        {allUnits.map((u) => (
          <Picker.Item key={u} label={unitLabel(u)} value={u} />
        ))}
      </Picker>

      <View style={{ flexDirection: "row", gap: 12, marginTop: 12 }}>
        <Button title="Cancel" onPress={onCancel} />
        <Button
          title="Save"
          onPress={() =>
            onSubmit({ name, quantity: Number(quantity) || undefined, unit })
          }
        />
      </View>
    </View>
  );
}
