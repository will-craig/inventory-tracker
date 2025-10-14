import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react-native";
import InventoryDetailScreen from "../../app/(protected)/inventory/[id]";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Provider as PaperProvider } from "react-native-paper";
import { SettingsContext, SettingsContextValue } from "../../providers/settings-context";
import { Alert } from "react-native";
import { Unit } from "../../services/clients/api-client";

jest.mock("../../services/inventory-service", () => ({
  InventoryService: {
    get: jest.fn(),
    save: jest.fn(),
    delete: jest.fn(),
  },
}));

const { InventoryService } = jest.requireMock("../../services/inventory-service");

function renderWithProviders(ui: React.ReactElement, settings?: Partial<SettingsContextValue>) {
  const client = new QueryClient();
  const defaultSettings: SettingsContextValue = {
    settings: { theme: "system", confirmDelete: true },
    setTheme: jest.fn(),
    setConfirmDelete: jest.fn(),
  };
  return render(
    <PaperProvider>
      <QueryClientProvider client={client}>
        <SettingsContext.Provider value={{ ...defaultSettings, ...settings }}>
          {ui}
        </SettingsContext.Provider>
      </QueryClientProvider>
    </PaperProvider>
  );
}

describe("InventoryDetailScreen", () => {
  beforeEach(() => {
    (jest.spyOn(Alert, "alert") as jest.Mock).mockImplementation(() => {});
  });
  afterEach(() => {
    jest.resetAllMocks();
  });

  test("loads item and saves changes", async () => {
    (InventoryService.get as jest.Mock).mockResolvedValue({
      id: "test-id",
      name: "Flour",
      quantity: 2,
      unit: Unit.None,
    });
    (InventoryService.save as jest.Mock).mockResolvedValue({});

    renderWithProviders(<InventoryDetailScreen />);

    // Wait for loaded values
    await waitFor(() => expect(screen.getByDisplayValue("Flour")).toBeTruthy());

    // Change name and save
    fireEvent.changeText(screen.getByDisplayValue("Flour"), "Flour 00");
    fireEvent.press(screen.getByTestId("save-button"));

    await waitFor(() => expect(InventoryService.save).toHaveBeenCalled());
    const savedArg = (InventoryService.save as jest.Mock).mock.calls[0][0];
    expect(savedArg.name).toBe("Flour 00");
  });

  test("validates name required", async () => {
    (InventoryService.get as jest.Mock).mockResolvedValue({
      id: "test-id",
      name: "",
      quantity: 0,
      unit: Unit.None,
    });

    renderWithProviders(<InventoryDetailScreen />);

    await waitFor(() => expect(screen.getByDisplayValue("")).toBeTruthy());
    fireEvent.press(screen.getByTestId("save-button"));

    expect(Alert.alert).toHaveBeenCalledWith("Validation", "Name is required");
    expect(InventoryService.save).not.toHaveBeenCalled();
  });

  test("delete with confirmation when enabled", async () => {
    (InventoryService.get as jest.Mock).mockResolvedValue({ id: "test-id", name: "Milk", quantity: 1, unit: Unit.None });
    (InventoryService.delete as jest.Mock).mockResolvedValue({});

    // Intercept Alert and immediately confirm
    (Alert.alert as jest.Mock).mockImplementation((title, message, buttons) => {
      const del = buttons?.find((b: any) => b.text === "Delete");
      del?.onPress?.();
    });

    renderWithProviders(<InventoryDetailScreen />, { settings: { theme: "system", confirmDelete: true } as any });

    await waitFor(() => expect(screen.getByDisplayValue("Milk")).toBeTruthy());
    fireEvent.press(screen.getByTestId("delete-button"));

    await waitFor(() => expect(InventoryService.delete).toHaveBeenCalledWith("test-id"));
  });

  test("delete without confirmation when disabled", async () => {
    (InventoryService.get as jest.Mock).mockResolvedValue({ id: "test-id", name: "Tea", quantity: 1, unit: Unit.None });
    (InventoryService.delete as jest.Mock).mockResolvedValue({});

    renderWithProviders(<InventoryDetailScreen />, { settings: { theme: "system", confirmDelete: false } as any });

    await waitFor(() => expect(screen.getByDisplayValue("Tea")).toBeTruthy());
    fireEvent.press(screen.getByTestId("delete-button"));

    await waitFor(() => expect(InventoryService.delete).toHaveBeenCalledWith("test-id"));
    expect(Alert.alert).not.toHaveBeenCalled();
  });
});
