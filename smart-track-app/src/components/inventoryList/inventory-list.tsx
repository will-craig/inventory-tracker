import React, { useEffect, useMemo, useState } from "react";
import { View } from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { RootStackParamList } from "../../App";
import { InventoryService } from "../../services/inventory-service";
import InventoryForm from "./inventory-form";
import { InventoryItem } from "../../domain/models/inventory-item";
import {
  ActivityIndicator,
  Button,
  Card,
  Dialog,
  FAB,
  List,
  Modal,
  Portal,
  Snackbar,
  Text,
} from "react-native-paper";

type Props = NativeStackScreenProps<RootStackParamList, "InventoryList">;

export default function InventoryList({ navigation }: Props) {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // create/edit modal
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<InventoryItem | null>(null);

  // delete confirm dialog
  const [deleteId, setDeleteId] = useState<string | null>(null);

  // snackbar
  const [snack, setSnack] = useState<{
    visible: boolean;
    msg: string;
    error?: boolean;
  }>({
    visible: false,
    msg: "",
  });

  const showSnack = (msg: string, error = false) =>
    setSnack({ visible: true, msg, error });

  const load = async () => {
    setError(null);
    try {
      const data = await InventoryService.list();
      setItems(data);
    } catch (e: any) {
      setError(e?.message ?? "Failed to load");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const onCreate = () => {
    setEditing(null);
    setModalOpen(true);
  };

  const onEdit = (item: InventoryItem) => {
    setEditing(item);
    setModalOpen(true);
  };

  const onDelete = (id?: string) => {
    if (!id) return;
    setDeleteId(id); // show dialog
  };

  const confirmDelete = async () => {
    if (!deleteId) return;
    try {
      // Optimistic update
      setItems((prev) => prev.filter((x) => x.id !== deleteId));
      await InventoryService.delete(deleteId);
      showSnack("Item deleted");
    } catch (e: any) {
      showSnack(e?.message ?? "Delete failed", true);
      // Reload to be safe if server failed
      await load();
    } finally {
      setDeleteId(null);
    }
  };

  const empty = useMemo(() => items.length === 0, [items]);

  if (loading) return <ActivityIndicator style={{ marginTop: 40 }} />;

  if (error)
    return (
      <View style={{ padding: 16 }}>
        <Text style={{ color: "crimson", marginBottom: 12 }}>{error}</Text>
        <Button mode="contained" onPress={load}>
          Retry
        </Button>
      </View>
    );

  return (
    <View style={{ flex: 1 }}>
      <List.Section>
        {empty ? (
          <Text style={{ padding: 16, opacity: 0.7 }}>
            No inventory items yet
          </Text>
        ) : (
          items.map((item) => {
            const hasId = !!item.id;
            return (
              <Card
                key={item.id ?? `temp-${item.name}-${Math.random()}`}
                style={{
                  marginHorizontal: 12,
                  marginVertical: 6,
                  opacity: hasId ? 1 : 0.6,
                }}
                onPress={() =>
                  hasId &&
                  navigation.navigate("InventoryDetail", { id: item.id! })
                }
              >
                <Card.Title
                  title={hasId ? item.name : `(unsaved) ${item.name}`}
                  subtitle={`${item.quantity ?? 0} ${String(item.unit ?? "")}`}
                  right={() => (
                    <View
                      style={{ flexDirection: "row", gap: 8, paddingRight: 8 }}
                    >
                      <Button compact onPress={() => onEdit(item)}>
                        Edit
                      </Button>
                      <Button
                        compact
                        textColor="#c00"
                        onPress={() => onDelete(item.id)}
                        disabled={!hasId}
                      >
                        Delete
                      </Button>
                    </View>
                  )}
                />
              </Card>
            );
          })
        )}
      </List.Section>

      {/* Add FAB */}
      <FAB
        icon="plus"
        label="Add item"
        onPress={onCreate}
        style={{ position: "absolute", right: 16, bottom: 24 }}
      />

      {/* Create/Edit Modal (Paper Modal wraps the content nicely on web + native) */}
      <Portal>
        <Modal
          visible={modalOpen}
          onDismiss={() => setModalOpen(false)}
          contentContainerStyle={{
            backgroundColor: "white",
            margin: 16,
            borderRadius: 12,
            padding: 12,
          }}
        >
          <InventoryForm
            initial={editing ?? undefined}
            onCancel={() => setModalOpen(false)}
            onSubmit={async (values) => {
              try {
                setSaving(true);
                const payload: InventoryItem = editing
                  ? { ...editing, ...values }
                  : (values as InventoryItem);
                await InventoryService.save(payload);
                setModalOpen(false);
                await load();
                showSnack(editing ? "Item updated" : "Item created");
              } catch (e: any) {
                showSnack(e?.message ?? "Save failed", true);
              } finally {
                setSaving(false);
              }
            }}
          />
        </Modal>
      </Portal>

      {/* Delete confirmation dialog */}
      <Portal>
        <Dialog visible={!!deleteId} onDismiss={() => setDeleteId(null)}>
          <Dialog.Title>Delete item?</Dialog.Title>
          <Dialog.Content>
            <Text>This action cannot be undone.</Text>
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={() => setDeleteId(null)}>Cancel</Button>
            <Button textColor="#c00" onPress={confirmDelete}>
              Delete
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>

      {/* Snackbar for feedback */}
      <Snackbar
        visible={snack.visible}
        onDismiss={() => setSnack({ visible: false, msg: "" })}
        duration={2500}
        style={snack.error ? { backgroundColor: "#b00020" } : undefined}
      >
        {snack.msg}
      </Snackbar>
    </View>
  );
}
