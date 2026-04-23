import type { JwtClaims } from "../types";

export const TOKEN_KEY = "art_auction_token";

/** Any of these in the JWT is enough to open the admin app shell (re-login after role changes). */
export const ADMIN_PORTAL_ACCESS_PERMISSIONS: readonly string[] = [
  "accounts.manage.artist",
  "approve.artist",
  "artworks.review",
  "approve.artwork",
  "users.view",
  "view.users",
  "roles.manage",
  "permissions.manage",
  "role.assignments.manage",
  "catalog.manage",
] as const;

export const PENDING_ARTIST_PERMS: readonly string[] = ["accounts.manage.artist", "approve.artist"];
export const PENDING_ARTWORK_PERMS: readonly string[] = ["artworks.review", "approve.artwork"];
export const USERS_TAB_PERMS: readonly string[] = ["users.view", "view.users", "role.assignments.manage"];
export const ROLES_TAB_PERMS: readonly string[] = ["roles.manage", "permissions.manage"];
export const CATALOG_TAB_PERMS: readonly string[] = ["catalog.manage"];

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function decodeJwt(token: string): JwtClaims {
  try {
    const payload = token.split(".")[1];
    const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(json) as JwtClaims;
  } catch {
    return {};
  }
}

export function permissionList(claims: JwtClaims): string[] {
  const permissions = claims.permission;
  if (!permissions) return [];
  if (Array.isArray(permissions)) return permissions;
  return [permissions];
}

export function hasPermission(claims: JwtClaims, permission: string): boolean {
  return permissionList(claims).includes(permission);
}

export function hasAnyPermission(claims: JwtClaims, names: readonly string[]): boolean {
  if (names.length === 0) return false;
  const set = new Set(permissionList(claims));
  return names.some((n) => set.has(n));
}

export function canAccessAdminPortal(claims: JwtClaims): boolean {
  return hasAnyPermission(claims, ADMIN_PORTAL_ACCESS_PERMISSIONS);
}
