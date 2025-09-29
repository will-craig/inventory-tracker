import React, { useEffect, useMemo, useRef, useState } from "react";
import { Keyboard, Platform, View } from "react-native";
import { Button, HelperText, Menu, Text, TextInput } from "react-native-paper";
import type { InventoryItem } from "../../domain/models/inventory-item";
import { Unit } from "../../services/clients/api-client";
type Props = {
    initial?: InventoryItem;
    onCancel: () => void;
    onSubmit: (values: Partial<InventoryItem>) => Promise<void> | void;
};

function toOptions(map: Record<number, string>) {
    // turn {1:'Part', 2:'Piece', ...} into [{value:1,label:'Part'}, ...]
    return Object.keys(map)
        .filter((k) => !Number.isNaN(Number(k)))
        .map((k) => ({ value: Number(k) as Unit, label: map[Number(k) as any] }));
}
function findUnitByLabel(map: Record<number, string>, label: string): Unit | undefined {
    const entry = Object.entries(map).find(([, v]) => v === label);
    return entry ? (Number(entry[0]) as Unit) : undefined;
}

export default function InventoryForm({ initial, onCancel, onSubmit }: Props) {
    // build options once
    const unitMap = unitLabel as unknown as Record<number, string>;
    const UNIT_OPTIONS = useMemo(() => toOptions(unitMap), [unitMap]);

    // default unit = “Part” (falls back to first option if not found)
    const defaultUnit: Unit =
        initial?.unit ??
        findUnitByLabel(unitMap, "Part") ??
        (UNIT_OPTIONS[0]?.value as Unit);

    // form state
    const [name, setName] = useState("");
    const [quantity, setQuantity] = useState("1");
    const [unit, setUnit] = useState<Unit>(defaultUnit);
    const [expiryDate, setExpiryDate] = useState<string>("");

    // ui state
    const [busy, setBusy] = useState(false);
    const [menuOpen, setMenuOpen] = useState(false);

    // refs for fast focusing
    const nameRef = useRef<any>(null);
    const qtyRef = useRef<any>(null);
    const dateRef = useRef<any>(null);

    // hydrate on open / edit switch
    useEffect(() => {
        setName(initial?.name ?? "");
        setQuantity(
            initial?.quantity != null && !Number.isNaN(initial.quantity)
                ? String(initial.quantity)
                : "1"
        );
        setUnit(initial?.unit ?? defaultUnit);
        setExpiryDate(
            initial?.expiryDate ? new Date(initial.expiryDate).toISOString().slice(0, 10) : ""
        );
    }, [initial]);

    // tiny validation
    const nameError = !name.trim() ? "Name is required" : "";
    const qtyError = quantity && !/^\d+$/.test(quantity) ? "Enter a whole number" : "";
    const dateError =
        expiryDate && !/^\d{4}-\d{2}-\d{2}$/.test(expiryDate) ? "Use YYYY-MM-DD" : "";

    const hasErrors = !!(nameError || qtyError || dateError);

    const handleSubmit = async (addAnother = false) => {
        if (hasErrors) return;
        setBusy(true);
        Keyboard.dismiss();

        const payload: Partial<InventoryItem> = {
            id: initial?.id, // present when editing
            name: name.trim(),
            quantity: quantity ? parseInt(quantity, 10) : undefined,
            unit, // <- enum value, mapper/toApi can pass straight through
            expiryDate: expiryDate ? new Date(expiryDate + "T00:00:00Z") : undefined,
        };

        try {
            await onSubmit(payload);
            if (addAnother && !initial?.id) {
                // fast reset for next entry
                setName("");
                setQuantity("1");
                setUnit(defaultUnit);
                setExpiryDate("");
                setTimeout(() => nameRef.current?.focus?.(), 50);
            }
        } finally {
            setBusy(false);
        }
    };

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
                error={!!nameError}
            />
            <HelperText type="error" visible={!!nameError}>
                {nameError}
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
                error={!!qtyError}
            />
            <HelperText type="error" visible={!!qtyError}>
                {qtyError}
            </HelperText>

            {/* Unit dropdown driven by unitLabel */}
            <View style={{ flexDirection: "row", alignItems: "center" }}>
                <Menu
                    visible={menuOpen}
                    onDismiss={() => setMenuOpen(false)}
                    anchor={
                        <Button mode="outlined" onPress={() => setMenuOpen(true)} style={{ flex: 1 }}>
                            Unit: {unitMap[unit] ?? "—"}
                        </Button>
                    }
                >
                    {UNIT_OPTIONS.map((opt) => (
                        <Menu.Item
                            key={String(opt.value)}
                            title={opt.label}
                            onPress={() => {
                                setUnit(opt.value);
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
                error={!!dateError}
            />
            <HelperText type="error" visible={!!dateError}>
                {dateError}
            </HelperText>

            {/* Actions */}
            <View style={{ flexDirection: "row", justifyContent: "flex-end", gap: 12, marginTop: 4 }}>
                <Button mode="text" onPress={onCancel} disabled={busy}>
                    Cancel
                </Button>
                {!initial?.id && (
                    <Button
                        mode="contained-tonal"
                        onPress={() => handleSubmit(true)}
                        disabled={busy || hasErrors}
                    >
                        Save & add another
                    </Button>
                )}
                <Button mode="contained" onPress={() => handleSubmit(false)} loading={busy} disabled={busy || hasErrors}>
                    Save
                </Button>
            </View>
        </View>
    );
}
