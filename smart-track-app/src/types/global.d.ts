declare const __DEV__: boolean;

// Minimal typing for process.env in Expo bundler
declare const process: {
  env?: { [key: string]: string | undefined };
};
