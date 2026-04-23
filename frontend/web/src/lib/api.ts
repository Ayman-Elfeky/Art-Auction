import type {
  ArtworkDetail,
  AuthTokenResponse,
  AuthUser,
  Bid,
  CreateArtworkInput,
  NotificationDto,
  PagedArtwork,
  PendingArtist,
  PendingArtwork,
} from "../types";

const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5161";

async function request<T>(
  path: string,
  options: RequestInit = {},
  token?: string | null,
): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  });

  if (!response.ok) {
    const text = await response.text();
    let message = text || `Request failed (${response.status})`;
    try {
      const json = JSON.parse(text);
      // Extract first readable message from various API error shapes
      message =
        json.error ||
        json.message ||
        json.title ||
        (Array.isArray(json.errors)
          ? json.errors.join(". ")
          : typeof json.errors === "object"
          ? Object.values(json.errors).flat().join(". ")
          : null) ||
        message;
    } catch {
      /* keep raw text */
    }
    throw new Error(message);
  }

  if (response.status === 204) return undefined as T;

  const contentType = response.headers.get("Content-Type");
  if (contentType && contentType.includes("application/json")) {
    return (await response.json()) as T;
  }
  return (await response.text()) as unknown as T;
}

// ── RBAC types ──────────────────────────────────────────────────────
export type RbacRole = {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  permissions: string[];
};

export type RbacPermission = {
  id: string;
  name: string;
  description: string;
};

export type RbacUser = {
  id: string;
  username: string;
  email: string;
  isApproved: boolean;
  isActive: boolean;
  role: string;
  createdAt: string;
};

export const api = {
  // ── Auth ────────────────────────────────────────────────────────
  login: (email: string, password: string) =>
    request<AuthTokenResponse>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),
  registerBuyer: (name: string, email: string, password: string) =>
    request<AuthTokenResponse>("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ name, email, password, role: "buyer" }),
    }),
  registerArtist: (name: string, email: string, password: string) =>
    request<AuthTokenResponse>("/api/auth/register/artist", {
      method: "POST",
      body: JSON.stringify({ name, email, password }),
    }),
  getMe: (token: string) => request<AuthUser>("/api/auth/me", { method: "GET" }, token),

  // ── Artworks ─────────────────────────────────────────────────────
  getArtworks: (query = "") => request<PagedArtwork>(`/api/artworks${query}`),
  getArtwork: (id: string) => request<ArtworkDetail>(`/api/artworks/${id}`),
  getArtworkBids: (id: string) => request<Bid[]>(`/api/bids/artworks/${id}`),
  getArtworksByArtist: (artistId: string, token: string, query = "") =>
    request<PagedArtwork>(`/api/artworks/artist/${artistId}${query}`, { method: "GET" }, token),
  getCategories: () =>
    request<Array<{ id: string; name: string; description: string }>>("/api/categories"),
  getTags: () => request<string[]>("/api/tags"),
  createArtwork: (token: string, payload: CreateArtworkInput) =>
    request("/api/artworks", { method: "POST", body: JSON.stringify(payload) }, token),
  updateArtwork: (token: string, artworkId: string, payload: CreateArtworkInput) =>
    request(`/api/artworks/${artworkId}`, { method: "PUT", body: JSON.stringify(payload) }, token),
  deleteArtwork: (token: string, artworkId: string) =>
    request(`/api/artworks/${artworkId}`, { method: "DELETE" }, token),
  extendAuction: (token: string, artworkId: string, newEndTime: string) =>
    request(`/api/artworks/${artworkId}/extend`, { method: "PUT", body: JSON.stringify({ newEndTime }) }, token),
  uploadImage: async (token: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    const response = await fetch(`${API_BASE}/api/uploads/images`, {
      method: "POST",
      headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) },
      body: form,
    });
    if (!response.ok) throw new Error(await response.text());
    return (await response.json()) as { imageUrl: string };
  },

  // ── Bids ─────────────────────────────────────────────────────────
  placeBid: (token: string, artId: string, amount: number) =>
    request<string>(
      "/api/bids",
      {
        method: "POST",
        body: JSON.stringify({
          id: crypto.randomUUID(),
          issuerId: "ignored-by-backend",
          artId,
          amount,
          highestBidder: false,
          placed: new Date().toISOString(),
        }),
      },
      token,
    ),

  // ── Watchlist ────────────────────────────────────────────────────
  getWatchlist: (token: string) => request<unknown[]>("/api/watchlist", { method: "GET" }, token),
  addWatchlist: (token: string, artworkId: string) =>
    request<{ message: string }>(`/api/watchlist/${artworkId}`, { method: "POST" }, token),
  removeWatchlist: (token: string, artworkId: string) =>
    request<{ success: boolean; message: string }>(`/api/watchlist/${artworkId}`, { method: "DELETE" }, token),

  // ── Notifications ────────────────────────────────────────────────
  getNotifications: (token: string) =>
    request<NotificationDto[]>("/api/notifications/me", { method: "GET" }, token),
  markNotificationRead: (token: string, id: string) =>
    request<{ message: string }>(`/api/notifications/${id}/read`, { method: "PUT" }, token),
  markAllNotificationsRead: (token: string) =>
    request<{ message: string }>("/api/notifications/read-all", { method: "PUT" }, token),

  // ── Admin – artist/artwork approval ─────────────────────────────
  getPendingArtists: (token: string) =>
    request<PendingArtist[]>("/api/admin/artists/pending", { method: "GET" }, token),
  approveArtist: (token: string, id: string) =>
    request<{ message: string }>(`/api/admin/artists/${id}/approve`, { method: "PUT" }, token),
  rejectArtist: (token: string, id: string) =>
    request<{ message: string }>(`/api/admin/artists/${id}/reject`, { method: "PUT" }, token),
  getPendingArtworks: (token: string) =>
    request<PendingArtwork[]>("/api/admin/artworks/pending", { method: "GET" }, token),
  approveArtwork: (token: string, id: string) =>
    request<{ message: string }>(`/api/admin/artworks/${id}/approve`, { method: "PUT" }, token),
  rejectArtwork: (token: string, id: string) =>
    request<{ message: string }>(`/api/admin/artworks/${id}/reject`, { method: "PUT" }, token),

  // ── RBAC – Roles ─────────────────────────────────────────────────
  getRoles: (token: string) =>
    request<RbacRole[]>("/api/rbac/roles", { method: "GET" }, token),
  createRole: (token: string, name: string, description: string, permissions: string[]) =>
    request<{ message: string; roleId: string }>(
      "/api/rbac/roles",
      { method: "POST", body: JSON.stringify({ name, description, permissions }) },
      token,
    ),
  updateRole: (token: string, roleId: string, name: string, description: string, permissions: string[]) =>
    request<{ message: string }>(
      `/api/rbac/roles/${roleId}`,
      { method: "PUT", body: JSON.stringify({ name, description, permissions }) },
      token,
    ),
  deleteRole: (token: string, roleId: string) =>
    request<{ message: string }>(`/api/rbac/roles/${roleId}`, { method: "DELETE" }, token),

  // ── RBAC – Permissions ───────────────────────────────────────────
  getPermissions: (token: string) =>
    request<RbacPermission[]>("/api/rbac/permissions", { method: "GET" }, token),
  createPermission: (token: string, name: string, description: string) =>
    request<{ message: string }>(
      "/api/rbac/permissions",
      { method: "POST", body: JSON.stringify({ name, description }) },
      token,
    ),
  updatePermission: (token: string, permissionId: string, name: string, description: string) =>
    request<{ message: string }>(
      `/api/rbac/permissions/${permissionId}`,
      { method: "PUT", body: JSON.stringify({ name, description }) },
      token,
    ),
  deletePermission: (token: string, permissionId: string) =>
    request<{ message: string }>(`/api/rbac/permissions/${permissionId}`, { method: "DELETE" }, token),

  // ── RBAC – Users ─────────────────────────────────────────────────
  getRbacUsers: (token: string) =>
    request<RbacUser[]>("/api/rbac/users", { method: "GET" }, token),
  assignRoleToUser: (token: string, userId: string, roleId: string) =>
    request<{ message: string }>(
      `/api/rbac/users/${userId}/role`,
      { method: "PUT", body: JSON.stringify({ roleId }) },
      token,
    ),

  // ── Admin – Categories CRUD ──────────────────────────────────────
  adminGetCategories: (token: string) =>
    request<Array<{ id: string; name: string; description: string }>>("/api/admin/catalog/categories", { method: "GET" }, token),
  adminCreateCategory: (token: string, name: string, description: string) =>
    request<{ message: string; id: string; name: string; description: string }>(
      "/api/admin/catalog/categories",
      { method: "POST", body: JSON.stringify({ name, description }) },
      token,
    ),
  adminUpdateCategory: (token: string, id: string, name: string, description: string) =>
    request<{ message: string }>(
      `/api/admin/catalog/categories/${id}`,
      { method: "PUT", body: JSON.stringify({ name, description }) },
      token,
    ),
  adminDeleteCategory: (token: string, id: string) =>
    request<{ message: string }>(`/api/admin/catalog/categories/${id}`, { method: "DELETE" }, token),

  // ── Admin – Tags CRUD ────────────────────────────────────────────
  adminGetTags: (token: string) =>
    request<string[]>("/api/admin/catalog/tags", { method: "GET" }, token),
  adminCreateTag: (token: string, name: string) =>
    request<{ message: string; tag: string }>(
      "/api/admin/catalog/tags",
      { method: "POST", body: JSON.stringify({ name }) },
      token,
    ),
  adminRenameTag: (token: string, oldTag: string, newName: string) =>
    request<{ message: string }>(
      `/api/admin/catalog/tags/${encodeURIComponent(oldTag)}`,
      { method: "PUT", body: JSON.stringify({ name: newName }) },
      token,
    ),
  adminDeleteTag: (token: string, tag: string) =>
    request<{ message: string }>(`/api/admin/catalog/tags/${encodeURIComponent(tag)}`, { method: "DELETE" }, token),
};
