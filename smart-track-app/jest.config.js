// Jest config for Expo + React Native Testing Library
/** @type {import('jest').Config} */
module.exports = {
  preset: "jest-expo",
  testMatch: ["**/?(*.)+(test).[jt]s?(x)"],
  setupFilesAfterEnv: ["<rootDir>/jest.setup.tsx", "@testing-library/jest-native/extend-expect"],
  transformIgnorePatterns: [
    "node_modules/(?!(react-native|@react-native|expo(nent)?|@expo(nent)?/.*|expo-status-bar|expo-modules-core|expo-router|@unimodules/.*|@react-native-community/.*|@react-navigation/.*|react-native-paper|react-native-vector-icons)/)",
  ],
  moduleNameMapper: {
    "^react-native$": "react-native",
  },
  collectCoverageFrom: [
    "src/**/*.{ts,tsx}",
    "!src/**/index.ts",
    "!src/app/**/_layout.tsx",
    "!src/app/providers/**",
    "!src/**/types.ts",
  ],
};