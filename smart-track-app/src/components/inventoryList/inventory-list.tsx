import React, { useEffect, useMemo, useState } from "react";
import {
  View,
  Text,
  FlatList,
  ActivityIndicator,
  Button,
  RefreshControl,
  Alert,
  Pressable,
  Modal,
} from "react-native";
import { NativeStackScreenProps } from "@react-navigation/native-stack";
import { RootStackParamList } from "../../App";
import { InventoryService } from "../../services/inventory.service";
import InventoryForm from "./inventory-form";
import { InventoryItem } from "../../domain/models/inventory-item";
type Props = NativeStackScreenProps<RootStackParamList, "InventoryList">;

export default function InventoryList({ navigation }: Props) {
  const [items, setItems] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<InventoryItem | null>(null);

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

  const onRefresh = () => {
    setRefreshing(true);
    load();
  };
  const onCreate = () => {
    setEditing(null);
    setModalOpen(true);
  };
  const onEdit = (item: InventoryItem) => {
    setEditing(item);
    setModalOpen(true);
  };

  const onDelete = async (id: string) => {
    Alert.alert("Delete item?", "This action cannot be undone.", [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: async () => {
          try {
            await InventoryService.delete(id);
            await load();
          } catch (e: any) {
            Alert.alert("Delete failed", e?.message ?? "Error");
          }
        },
      },
    ]);
  };

  const empty = useMemo(() => items.length === 0, [items]);

  if (loading) return <ActivityIndicator style={{ marginTop: 40 }} />;
  if (error)
    return (
      <View style={{ padding: 16 }}>
        <Text style={{ color: "crimson" }}>{error}</Text>
        <Button title="Retry" onPress={load} />
      </View>
    );

  return (
    <View style={{ flex: 1 }}>
      <FlatList
        data={items}
        keyExtractor={(item, index) => item.id ?? `temp-${index}`} // works for drafts
        renderItem={({ item, index }) => {
          const hasId = !!item.id;
          return (
            <Pressable
              disabled={!hasId}
              onPress={() =>
                hasId &&
                navigation.navigate("InventoryDetail", { id: item.id! })
              }
              style={{
                padding: 12,
                borderBottomWidth: 1,
                borderColor: "#eee",
                opacity: hasId ? 1 : 0.6,
              }}
            >
              <Text style={{ fontWeight: "600" }}>{item.name}</Text>
              <Text>
                {item.quantity ?? 0} {String(item.unit ?? "")}
              </Text>

              <View style={{ flexDirection: "row", gap: 8, marginTop: 8 }}>
                <Button title="Edit" onPress={() => onEdit(item)} />
                <Button
                  title="Delete"
                  onPress={() => hasId && onDelete(item.id!)}
                  disabled={!hasId}
                />
              </View>
            </Pressable>
          );
        }}
      />

      <Modal
        visible={modalOpen}
        animationType="slide"
        onRequestClose={() => setModalOpen(false)}
      >
        <InventoryForm
          initial={editing ?? undefined}
          onCancel={() => setModalOpen(false)}
          onSubmit={async (values) => {
            try {
              //if (editing) await InventoryService.save(editing.id, values);
              //else await InventoryService.save(values);
              setModalOpen(false);
              await load();
            } catch (e: any) {
              Alert.alert("Save failed", e?.message ?? "Error");
            }
          }}
        />
      </Modal>
    </View>
  );
}
