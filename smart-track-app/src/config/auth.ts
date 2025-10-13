export type EntraConfig = {
  // Your Entra External ID (B2C) tenant primary domain, e.g. contoso.onmicrosoft.com
  tenantDomain: string;
  // Your Entra policy/user flow, e.g. B2C_1_signupsignin
  policy: string;
  // App registration (native/mobile) Application (client) ID
  clientId: string;
  // Requested scopes (include offline_access to enable refresh tokens)
  scopes: string[];
  // App scheme that matches app.json > expo.scheme
  scheme: string;
  // Optional: URL to return after logout
  postLogoutRedirectUri?: string;

  // Derived below
  authority: string;
  discoveryEndpoint: string;
};

// TODO: Fill these values from your Entra External ID tenant/app registration
const tenantDomain = "YOUR_TENANT.onmicrosoft.com"; // e.g., contoso.onmicrosoft.com
const policy = "B2C_1_signupsignin"; // e.g., B2C_1_signupsignin
const clientId = "00000000-0000-0000-0000-000000000000"; // Application (client) ID
const scheme = "smarttrack"; // Must match app.json expo.scheme
// Example API scope: "https://{tenantDomain}/{app-id-uri}/access_as_user" or a custom scope from your B2C API
const scopes = [
  "openid",
  "profile",
  "offline_access",
  // "https://YOUR_TENANT.onmicrosoft.com/YOUR_API/scope_name",
];

const base = `https://${tenantDomain.split(".")[0]}.b2clogin.com/${tenantDomain}/${policy}/v2.0`;

export const entraConfig: EntraConfig = {
  tenantDomain,
  policy,
  clientId,
  scheme,
  scopes,
  authority: base,
  discoveryEndpoint: `${base}/.well-known/openid-configuration`,
  // Set if you want a post-logout redirect
  // postLogoutRedirectUri: `${scheme}://logout`,
};
