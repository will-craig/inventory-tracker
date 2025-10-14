import { IApiClient, InventoryItemRequest, InventoryItemResponse, Unit } from "./api-client";

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function guid() {
  // Simple GUID generator (not cryptographically secure)
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}

const seedData = [
  {
    id: guid(),
    name: "Milk",
    quantity: 2,
    unit: Unit.Litre,
    expiryDate: new Date(Date.now() + 3 * 24 * 3600 * 1000), // in 3 days
  },
  {
    id: guid(),
    name: "Eggs",
    quantity: 12,
    unit: Unit.Part,
    expiryDate: new Date(Date.now() + 10 * 24 * 3600 * 1000),
  },
  {
    id: guid(),
    name: "Flour",
    quantity: 1,
    unit: Unit.Kilogram,
    expiryDate: undefined,
  },
] as const;

export class MockApiClient implements IApiClient {
  private items: InventoryItemResponse[] = [];
  private latency = 200; // ms

  constructor() {
    // Deep copy to avoid shared references
    this.items = seedData.map((i) => InventoryItemResponse.fromJS(i));
  }

  async login(): Promise<string> {
    await delay(this.latency);
    return "mock-token";
  }

  async getStock(): Promise<InventoryItemResponse[]> {
    await delay(this.latency);
    // Return copies to mimic API serialization
    return this.items.map((i) => InventoryItemResponse.fromJS(i));
  }

  async addStock(body?: InventoryItemRequest | undefined): Promise<InventoryItemResponse> {
    await delay(this.latency);
    const entity: InventoryItemResponse = InventoryItemResponse.fromJS({
      id: guid(),
      name: body?.name ?? "",
      quantity: body?.quantity ?? 0,
      unit: body?.unit ?? Unit.None,
      expiryDate: body?.expiryDate,
    });
    this.items.push(entity);
    return InventoryItemResponse.fromJS(entity);
  }

  async getStockById(id: string): Promise<InventoryItemResponse> {
    await delay(this.latency);
    const found = this.items.find((x) => x.id === id);
    if (!found) throw new Error("Not Found");
    return InventoryItemResponse.fromJS(found);
  }

  async updateStock(id: string, body?: InventoryItemRequest | undefined): Promise<InventoryItemResponse> {
    await delay(this.latency);
    const idx = this.items.findIndex((x) => x.id === id);
    if (idx < 0) throw new Error("Not Found");
    const updated: InventoryItemResponse = InventoryItemResponse.fromJS({
      id,
      name: body?.name ?? this.items[idx].name,
      quantity: body?.quantity ?? this.items[idx].quantity,
      unit: body?.unit ?? this.items[idx].unit,
      expiryDate: body?.expiryDate ?? this.items[idx].expiryDate,
    });
    this.items[idx] = updated;
    return InventoryItemResponse.fromJS(updated);
  }

  async deleteStock(id: string): Promise<void> {
    await delay(this.latency);
    this.items = this.items.filter((x) => x.id !== id);
  }
}