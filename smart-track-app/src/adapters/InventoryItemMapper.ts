import {
  InventoryItemRequest,
  InventoryItemResponse,
  Unit,
} from "../services/clients/api-client";
import { InventoryItem } from "../domain/models/inventory-item";

export function fromApi(dto: InventoryItemResponse): InventoryItem {
  return {
    id: dto.id,
    name: dto.name ?? "",
    quantity: dto.quantity ?? 0,
    unit: dto.unit ?? Unit.None,
    expiryDate: dto.expiryDate ? new Date(dto.expiryDate) : undefined,
  };
}

export function toApi(input: Partial<InventoryItem>): InventoryItemRequest {
  return InventoryItemRequest.fromJS({
    name: input.name,
    quantity: input.quantity,
    unit: input.unit,
    expiryDate: input.expiryDate,
  });
}
