import * as AuthSession from "expo-auth-session";
import * as WebBrowser from "expo-web-browser";
import { Platform } from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { entraConfig } from "../config/auth";

WebBrowser.maybeCompleteAuthSession();

const STORAGE_KEY = "auth.tokens";

type TokenState = {
  accessToken: string | null;
  idToken?: string | null;
  refreshToken?: string | null;
  expiresAt?: number | null; // epoch seconds
};

function nowSeconds() {
  return Math.floor(Date.now() / 1000);
}

function isExpired(expiresAt?: number | null) {
  if (!expiresAt) return true;
  // Refresh a minute early to be safe
  return expiresAt - nowSeconds() < 60;
}

async function saveTokens(tokens: TokenState | null) {
  if (!tokens) {
    await AsyncStorage.removeItem(STORAGE_KEY);
    return;
  }
  await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
}

async function loadTokens(): Promise<TokenState | null> {
  const json = await AsyncStorage.getItem(STORAGE_KEY);
  if (!json) return null;
  try {
    return JSON.parse(json) as TokenState;
  } catch {
    return null;
  }
}

function makeRedirectUri() {
  // In Expo Go/dev, proxy is used on web; for native, custom scheme
  return AuthSession.makeRedirectUri({
    scheme: entraConfig.scheme,
    // If you use Expo Router in web, set useProxy true for web; for native use direct scheme
    useProxy: Platform.OS === "web",
  });
}

export async function signInAsync(): Promise<boolean> {
  const discovery = await AuthSession.fetchDiscoveryAsync(entraConfig.discoveryEndpoint);

  const redirectUri = makeRedirectUri();

  const request = new AuthSession.AuthRequest({
    clientId: entraConfig.clientId,
    responseType: AuthSession.ResponseType.Code,
    usePKCE: true,
    scopes: entraConfig.scopes,
    redirectUri,
    // Extra params are usually not needed if policy is encoded in authority
  });

  await request.makeAuthUrlAsync(discovery);

  const result = await request.promptAsync(discovery, { useProxy: Platform.OS === "web" });
  if (result.type !== "success" || !result.params?.code) {
    return false;
  }

  const token = await AuthSession.exchangeCodeAsync(
    {
      clientId: entraConfig.clientId,
      code: result.params.code,
      redirectUri,
      extraParams: { code_verifier: request.codeVerifier ?? "" },
    },
    discovery,
  );

  const issuedAt = token.issuedAt ?? nowSeconds();
  const expiresAt = issuedAt + (token.expiresIn ?? 0);

  await saveTokens({
    accessToken: token.accessToken ?? null,
    idToken: token.idToken ?? null,
    refreshToken: token.refreshToken ?? null,
    expiresAt,
  });

  return true;
}

export async function signOutAsync(): Promise<void> {
  // Clear local session first
  await saveTokens(null);

  // Optional: initiate B2C logout in the browser
  try {
    const redirectUri = entraConfig.postLogoutRedirectUri ?? makeRedirectUri();
    const logoutUrl = `${entraConfig.authority}/oauth2/v2.0/logout?post_logout_redirect_uri=${encodeURIComponent(
      redirectUri,
    )}`;
    // Fire and forget; do not block logout on this
    WebBrowser.openBrowserAsync(logoutUrl);
  } catch {
    // no-op
  }
}

export async function getAccessToken(): Promise<string | null> {
  let tokens = await loadTokens();
  if (!tokens?.accessToken) return null;

  if (!isExpired(tokens.expiresAt)) {
    return tokens.accessToken;
  }

  // Attempt refresh if available
  if (!tokens.refreshToken) return null;

  const discovery = await AuthSession.fetchDiscoveryAsync(entraConfig.discoveryEndpoint);
  try {
    const refreshed = await AuthSession.refreshAsync(
      {
        clientId: entraConfig.clientId,
        refreshToken: tokens.refreshToken,
        scopes: entraConfig.scopes,
      },
      discovery,
    );

    const issuedAt = refreshed.issuedAt ?? nowSeconds();
    const expiresAt = issuedAt + (refreshed.expiresIn ?? 0);

    tokens = {
      accessToken: refreshed.accessToken ?? null,
      idToken: refreshed.idToken ?? tokens.idToken ?? null,
      refreshToken: refreshed.refreshToken ?? tokens.refreshToken ?? null,
      expiresAt,
    };

    await saveTokens(tokens);
    return tokens.accessToken;
  } catch (e) {
    // Refresh failed; clear tokens
    console.error("Token refresh failed:", e);
    await saveTokens(null);
    return null;
  }
}

export async function hasValidSession(): Promise<boolean> {
  const tokens = await loadTokens();
  return !!tokens?.accessToken && !isExpired(tokens.expiresAt);
}
