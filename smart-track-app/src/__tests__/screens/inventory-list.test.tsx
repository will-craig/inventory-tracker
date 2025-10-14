import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react-native";
import InventoryListScreen from "../../app/(protected)/inventory/index";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as PaperProvider } from "react-native-paper";

jest.mock("../../services/inventory-service", () => ({
  InventoryService: {
    list: jest.fn().mockResolvedValue([
      { id: "1", name: "Milk", quantity: 2, unit: 0 },
      { id: "2", name: "Sugar", quantity: 1, unit: 0 },
    ]),
  },
}));

describe("InventoryListScreen", () => {
  test("renders and filters list", async () => {
    const client = new QueryClient();
    render(
      <PaperProvider>
        <QueryClientProvider client={client}>
          <InventoryListScreen />
        </QueryClientProvider>
      </PaperProvider>
    );

    await waitFor(() => {
      expect(screen.getByText("Milk")).toBeTruthy();
      expect(screen.getByText("Sugar")).toBeTruthy();
    });

    const search = screen.getByPlaceholderText("Search inventory");
    fireEvent.changeText(search, "Mil");

    await waitFor(() => {
      expect(screen.getByText("Milk")).toBeTruthy();
      expect(screen.queryByText("Sugar")).toBeNull();
    });
  });
});
