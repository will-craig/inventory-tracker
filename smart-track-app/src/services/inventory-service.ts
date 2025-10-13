import { api } from "../services/api";
import { InventoryItem } from "../domain/models/inventory-item";
import { fromApi, toApi } from "../adapters/InventoryItemMapper";

export const InventoryService = {
  async list(): Promise<InventoryItem[]> {
    const stock = await api.getStock();
    return stock.map((i) => fromApi(i));
  },

  async get(id: string): Promise<InventoryItem> {
    const stockItem = await api.getStockById(id);
    return fromApi(stockItem);
  },

  async delete(id: string): Promise<void> {
    return await api.deleteStock(id);
  },

  async save(item: InventoryItem): Promise<InventoryItem> {
    const itemPayload = toApi(item);
    const result = item.id
      ? await api.updateStock(item.id, itemPayload) // update
      : await api.addStock(itemPayload); // create
    return fromApi(result);
  },
};
