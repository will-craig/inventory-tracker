import { Unit } from "../../services/clients/api-client";

export interface InventoryItem {
  id?: string;
  name: string;
  quantity: number;
  expiryDate?: Date;
  unit: Unit;
}
