import {
  authorize,
  refresh,
  revoke,
  AuthConfiguration,
} from "react-native-app-auth";
import * as Keychain from "react-native-keychain";
import Config from "react-native-config";

const config: AuthConfiguration = {
  issuer: Config.AUTHORITY,
  clientId: Config.CLIENT_ID ?? '',
  redirectUrl: Config.REDIRECT_URI ?? '',
  scopes: [ Config.SCOPE ?? '',
  ],
  serviceConfiguration: {
    authorizationEndpoint: `${Config.AUTHORITY}/oauth2/v2.0/authorize`,
    tokenEndpoint: `${Config.AUTHORITY}/oauth2/v2.0/token`,
  },
};

const KEY = { refresh: 'auth.refresh' };

// Interactive login
export async function signIn(): Promise<void> {
    console.log('Starting interactive sign-in');     
    console.log('Auth Config:', config);
  const res = await authorize(config);
  if (res.refreshToken) {
    await Keychain.setGenericPassword('refresh', res.refreshToken, { service: KEY.refresh });
  }
}

// Get a fresh access token silently
export async function getAccessToken(): Promise<string | undefined> {
  const credential = await Keychain.getGenericPassword({ service: KEY.refresh });
  if (credential === false) 
    return undefined;

  try {
    const res = await refresh(config, { refreshToken: credential.password });

    if (res.refreshToken && res.refreshToken !== credential.password) {
      await Keychain.setGenericPassword('refresh', res.refreshToken, { service: KEY.refresh });
    }
    return res.accessToken;
  } catch {
    return undefined; // caller can trigger interactive sign-in
  }
}

// revoke + clear)
export async function signOut(): Promise<void> {
  const credential = await Keychain.getGenericPassword({ service: KEY.refresh });
  if (credential !== false) {
    try { await revoke(config, { tokenToRevoke: credential.password, sendClientId: true }); } catch {}
  }
  await Keychain.resetGenericPassword({ service: KEY.refresh });
}

// check if we have a stored refresh token
export async function hasSession(): Promise<boolean> {
  const credential = await Keychain.getGenericPassword({ service: KEY.refresh });
  return !!credential;
}