import "@testing-library/jest-native/extend-expect";
import mockAsyncStorage from "@react-native-async-storage/async-storage/jest/async-storage-mock";
import React from "react";

// AsyncStorage mock
jest.mock("@react-native-async-storage/async-storage", () => mockAsyncStorage);

// expo-router basic mocks
jest.mock("expo-router", () => {
  return {
    useRouter: () => ({ push: jest.fn(), replace: jest.fn(), back: jest.fn() }),
    useLocalSearchParams: () => ({ id: "test-id" }),
    Link: ({ children }: any) => <>{children}</>,
    Stack: ({ children }: any) => <>{children}</>,
  };
});

// Mock DateTimePicker to a simple component
jest.mock("@react-native-community/datetimepicker", () => {
  return ({ value, onChange }: any) => null;
});

// Mock Picker to a simple component
jest.mock("@react-native-picker/picker", () => {
  const Picker = ({ children }: any) => null;
  Picker.Item = ({ label }: any) => null;
  return { Picker };
});