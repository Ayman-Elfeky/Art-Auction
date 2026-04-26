import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import {
  CATALOG_TAB_PERMS,
  PENDING_ARTIST_PERMS,
  PENDING_ARTWORK_PERMS,
  ROLES_TAB_PERMS,
  USERS_TAB_PERMS,
  hasAnyPermission,
  hasPermission,
} from "../lib/auth";
import { api, type RbacPermission, type RbacRole, type RbacUser } from "../lib/api";
import type { JwtClaims, PendingArtist, PendingArtwork } from "../types";

function CatalogTab({ token }: { token: string }) {
  const { t } = useLang();
  const [categories, setCategories] = useState<{ id: string; name: string; description: string }[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const [catName, setCatName] = useState("");
  const [catDesc, setCatDesc] = useState("");
  const [editCat, setEditCat] = useState<{ id: string; name: string; description: string } | null>(null);
  const [newTag, setNewTag] = useState("");
  const [renameTag, setRenameTag] = useState<{ old: string; val: string } | null>(null);
  const [msg, setMsg] = useState("");

  async function load() {
    const [cats, tgs] = await Promise.all([api.adminGetCategories(token), api.adminGetTags(token)]);
    setCategories(cats); setTags(tgs);
  }
  useEffect(() => { load().catch(() => undefined); }, [token]);

  function flash(m: string) { setMsg(m); setTimeout(() => setMsg(""), 3000); }

  async function saveCat() {
    if (!catName.trim()) return;
    try {
      if (editCat) {
        await api.adminUpdateCategory(token, editCat.id, catName, catDesc);
        flash(t("Category updated.", "تم تحديث التصنيف."));
      } else {
        await api.adminCreateCategory(token, catName, catDesc);
        flash(t("Category created.", "تم إنشاء التصنيف."));
      }
      setCatName(""); setCatDesc(""); setEditCat(null); await load();
    } catch (e) { flash((e as Error).message); }
  }

  async function deleteCat(id: string) {
    if (!confirm(t("Delete this category?", "حذف هذا التصنيف؟"))) return;
    try { await api.adminDeleteCategory(token, id); flash(t("Category deleted.", "تم الحذف.")); await load(); }
    catch (e) { flash((e as Error).message); }
  }

  async function createTag() {
    if (!newTag.trim()) return;
    try { await api.adminCreateTag(token, newTag); flash(t("Tag created.", "تم إنشاء الوسم.")); setNewTag(""); await load(); }
    catch (e) { flash((e as Error).message); }
  }

  async function saveRename() {
    if (!renameTag) return;
    try { await api.adminRenameTag(token, renameTag.old, renameTag.val); flash(t("Tag renamed.", "تمت إعادة التسمية.")); setRenameTag(null); await load(); }
    catch (e) { flash((e as Error).message); }
  }

  async function deleteTag(tag: string) {
    if (!confirm(t("Delete this tag from ALL artworks?", "حذف هذا الوسم من كل الأعمال؟"))) return;
    try { await api.adminDeleteTag(token, tag); flash(t("Tag deleted.", "تم حذف الوسم.")); await load(); }
    catch (e) { flash((e as Error).message); }
  }

  return (
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1.5rem", alignItems: "start" }}>
      {msg && <p className="success-msg" style={{ gridColumn: "1/-1" }}>{msg}</p>}

      {/* ── Categories ── */}
      <div>
        <h3 style={{ fontFamily: "'Cinzel',serif", fontSize: "1rem", color: "var(--gold-400)", marginBottom: "1rem" }}>
          {t("Categories", "التصنيفات")}
        </h3>

        {/* Form */}
        <div className="card" style={{ marginBottom: "1rem", display: "flex", flexDirection: "column", gap: "0.75rem" }}>
          <p style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>
            {editCat ? `${t("Editing:", "تعديل:")} ${editCat.name}` : t("New Category", "تصنيف جديد")}
          </p>
          <div className="field">
            <label>{t("Name", "الاسم")}</label>
            <input value={catName} onChange={(e) => setCatName(e.target.value)} placeholder="e.g. Paintings" />
          </div>
          <div className="field">
            <label>{t("Description", "الوصف")}</label>
            <input value={catDesc} onChange={(e) => setCatDesc(e.target.value)} placeholder={t("Optional", "اختياري")} />
          </div>
          <div className="row">
            <button className="btn btn-primary btn-sm" onClick={saveCat} type="button">
              {editCat ? t("Update", "تحديث") : t("Create", "إنشاء")}
            </button>
            {editCat && (
              <button className="btn btn-secondary btn-sm" onClick={() => { setEditCat(null); setCatName(""); setCatDesc(""); }} type="button">
                {t("Cancel", "إلغاء")}
              </button>
            )}
          </div>
        </div>

        {/* List */}
        <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
          {categories.map((c) => (
            <div key={c.id} className="card" style={{ padding: "0.75rem 1rem", display: "flex", alignItems: "center", gap: "0.75rem" }}>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 600, color: "var(--text-primary)", fontSize: "0.9rem" }}>{c.name}</div>
                {c.description && <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{c.description}</div>}
              </div>
              <button className="btn btn-secondary btn-sm" onClick={() => { setEditCat(c); setCatName(c.name); setCatDesc(c.description); }} type="button">
                {t("Edit", "تعديل")}
              </button>
              <button className="btn btn-danger btn-sm" onClick={() => deleteCat(c.id)} type="button">×</button>
            </div>
          ))}
        </div>
      </div>

      {/* ── Tags ── */}
      <div>
        <h3 style={{ fontFamily: "'Cinzel',serif", fontSize: "1rem", color: "var(--gold-400)", marginBottom: "1rem" }}>
          {t("Tags", "الوسوم")}
        </h3>

        {/* Create tag */}
        <div className="card" style={{ marginBottom: "1rem", display: "flex", flexDirection: "column", gap: "0.75rem" }}>
          <p style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>{t("Add new tag", "إضافة وسم جديد")}</p>
          <div className="field">
            <label>{t("Tag name", "اسم الوسم")}</label>
            <input value={newTag} onChange={(e) => setNewTag(e.target.value)} placeholder="e.g. watercolour" />
          </div>
          <button className="btn btn-primary btn-sm" onClick={createTag} type="button">{t("Add Tag", "إضافة وسم")}</button>
        </div>

        {/* Rename modal (inline) */}
        {renameTag && (
          <div className="card" style={{ marginBottom: "1rem", display: "flex", flexDirection: "column", gap: "0.75rem", borderColor: "var(--gold-500)" }}>
            <p style={{ fontSize: "0.82rem", color: "var(--gold-400)" }}>{t("Rename:", "إعادة تسمية:")} <strong>{renameTag.old}</strong></p>
            <div className="field">
              <label>{t("New name", "الاسم الجديد")}</label>
              <input value={renameTag.val} onChange={(e) => setRenameTag({ ...renameTag, val: e.target.value })} />
            </div>
            <div className="row">
              <button className="btn btn-primary btn-sm" onClick={saveRename} type="button">{t("Save", "حفظ")}</button>
              <button className="btn btn-secondary btn-sm" onClick={() => setRenameTag(null)} type="button">{t("Cancel", "إلغاء")}</button>
            </div>
          </div>
        )}

        {/* Tag list */}
        <div style={{ display: "flex", flexWrap: "wrap", gap: "0.5rem" }}>
          {tags.map((tag) => (
            <div key={tag} style={{ display: "flex", alignItems: "center", gap: "0.3rem", background: "rgba(201,162,39,0.1)", border: "1px solid var(--border-card)", borderRadius: "var(--radius-pill)", padding: "0.25rem 0.6rem" }}>
              <span style={{ fontSize: "0.8rem", color: "var(--gold-400)", fontWeight: 500 }}>{tag}</span>
              <button
                style={{ background: "none", border: "none", color: "var(--text-muted)", cursor: "pointer", fontSize: "0.85rem", padding: "0 2px", boxShadow: "none", transform: "none" }}
                onClick={() => setRenameTag({ old: tag, val: tag })}
                title={t("Rename", "إعادة تسمية")}
                type="button"
              >✏️</button>
              <button
                style={{ background: "none", border: "none", color: "#f87171", cursor: "pointer", fontSize: "0.85rem", padding: "0 2px", boxShadow: "none", transform: "none" }}
                onClick={() => deleteTag(tag)}
                title={t("Delete", "حذف")}
                type="button"
              >×</button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
// ── Pending Artists tab ──────────────────────────────────────────
function ArtistsTab({ token }: { token: string }) {
  const { t } = useLang();
  const [artists, setArtists] = useState<PendingArtist[]>([]);
  const [msg, setMsg] = useState("");

  async function load() { setArtists(await api.getPendingArtists(token)); }
  useEffect(() => { load().catch(() => undefined); }, [token]);

  async function act(fn: Promise<{ message: string }>) {
    try { const r = await fn; setMsg(r.message); await load(); }
    catch (e) { setMsg((e as Error).message); }
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem" }}>
      {msg && <p className="success-msg">{msg}</p>}
      {artists.length === 0 && <p style={{ color: "var(--text-muted)" }}>{t("No pending artists.", "لا فنانين في الانتظار.")}</p>}
      {artists.map((a) => (
        <div key={a.id} className="card" style={{ display: "flex", alignItems: "center", gap: "1rem", padding: "1rem 1.2rem" }}>
          <div style={{ width: 44, height: 44, borderRadius: "50%", background: "rgba(201,162,39,0.15)", border: "1px solid var(--border-card)", display: "flex", alignItems: "center", justifyContent: "center", fontFamily: "'Cinzel',serif", fontWeight: 700, color: "var(--gold-400)", fontSize: "1.1rem", flexShrink: 0 }}>
            {a.username.charAt(0).toUpperCase()}
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontWeight: 600, color: "var(--text-primary)" }}>{a.username}</div>
            <div style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>{a.email} · {new Date(a.createdAt).toLocaleDateString()}</div>
          </div>
          <button className="btn btn-success btn-sm" onClick={() => act(api.approveArtist(token, a.id))} type="button">{t("Approve", "موافقة")}</button>
          <button className="btn btn-danger btn-sm" onClick={() => act(api.rejectArtist(token, a.id))} type="button">{t("Reject", "رفض")}</button>
        </div>
      ))}
    </div>
  );
}

// ── Pending Artworks tab ─────────────────────────────────────────
function ArtworksTab({ token }: { token: string }) {
  const { t } = useLang();
  const [artworks, setArtworks] = useState<PendingArtwork[]>([]);
  const [msg, setMsg] = useState("");

  async function load() { setArtworks(await api.getPendingArtworks(token)); }
  useEffect(() => { load().catch(() => undefined); }, [token]);

  async function act(fn: Promise<{ message: string }>) {
    try { const r = await fn; setMsg(r.message); await load(); }
    catch (e) { setMsg((e as Error).message); }
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem" }}>
      {msg && <p className="success-msg">{msg}</p>}
      {artworks.length === 0 && <p style={{ color: "var(--text-muted)" }}>{t("No pending artworks.", "لا أعمال في الانتظار.")}</p>}
      {artworks.map((aw) => (
        <Link to={`/artworks/${aw.id}`} key={aw.id} style={{ textDecoration: "none", color: "inherit" }}>
          <div className="card" style={{ display: "flex", alignItems: "center", gap: "1rem", padding: "1rem 1.2rem" }}>
            <div style={{ flex: 1 }}>
              <span style={{ fontWeight: 600, color: "var(--text-primary)", fontSize: "0.95rem" }}>
                {aw.title}
              </span>
              <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginTop: "0.15rem" }}>
                {t("by", "بقلم")} {aw.artistName} · ${aw.initialPrice} · {new Date(aw.createdAt).toLocaleDateString()}
              </div>
              <div style={{ fontSize: "0.8rem", color: "var(--text-secondary)", marginTop: "0.3rem" }}>{aw.description?.slice(0, 120)}…</div>
            </div>
            <button className="btn btn-success btn-sm" onClick={() => act(api.approveArtwork(token, aw.id))} type="button">{t("Approve", "موافقة")}</button>
            <button className="btn btn-danger btn-sm" onClick={() => act(api.rejectArtwork(token, aw.id))} type="button">{t("Reject", "رفض")}</button>
          </div>
        </Link>
      ))}
    </div>
  );
}

// ── Roles & Permissions tab ──────────────────────────────────────
function RolesTab({ token, claims }: { token: string; claims: JwtClaims }) {
  const { t } = useLang();
  const canListRoles = hasAnyPermission(claims, ["roles.manage", "role.assignments.manage", "permissions.manage"]);
  const canListPerms = hasAnyPermission(claims, ["permissions.manage", "roles.manage"]);
  const canManageRoles = hasPermission(claims, "roles.manage");
  const canManagePerms = hasPermission(claims, "permissions.manage");
  const [roles, setRoles] = useState<RbacRole[]>([]);
  const [perms, setPerms] = useState<RbacPermission[]>([]);
  const [editRole, setEditRole] = useState<RbacRole | null>(null);
  const [newRoleName, setNewRoleName] = useState("");
  const [newRoleDesc, setNewRoleDesc] = useState("");
  const [newRolePerms, setNewRolePerms] = useState<string[]>([]);
  const [newPermName, setNewPermName] = useState("");
  const [newPermDesc, setNewPermDesc] = useState("");
  const [msg, setMsg] = useState("");

  const load = useCallback(async () => {
    if (!canListRoles && !canListPerms) return;
    const part: Promise<void>[] = [];
    if (canListRoles) {
      part.push(
        api.getRoles(token).then((r) => {
          setRoles(r);
        }),
      );
    }
    if (canListPerms) {
      part.push(
        api.getPermissions(token).then((p) => {
          setPerms(p);
        }),
      );
    }
    await Promise.all(part);
  }, [canListPerms, canListRoles, token]);

  useEffect(() => {
    load().catch(() => undefined);
  }, [load]);

  function togglePerm(name: string) {
    setNewRolePerms((p) => p.includes(name) ? p.filter((x) => x !== name) : [...p, name]);
  }

  async function saveRole() {
    try {
      if (editRole) {
        await api.updateRole(token, editRole.id, newRoleName, newRoleDesc, newRolePerms);
        setMsg(t("Role updated.", "تم تحديث الدور."));
      } else {
        await api.createRole(token, newRoleName, newRoleDesc, newRolePerms);
        setMsg(t("Role created.", "تم إنشاء الدور."));
      }
      setEditRole(null); setNewRoleName(""); setNewRoleDesc(""); setNewRolePerms([]);
      await load();
    } catch (e) { setMsg((e as Error).message); }
  }

  function startEdit(role: RbacRole) {
    setEditRole(role); setNewRoleName(role.name); setNewRoleDesc(role.description);
    setNewRolePerms([...role.permissions]);
  }

  async function deleteRole(id: string, isSystem: boolean) {
    if (isSystem) { setMsg(t("Cannot delete system roles.", "لا يمكن حذف أدوار النظام.")); return; }
    if (!confirm(t("Delete this role?", "حذف هذا الدور؟"))) return;
    try { await api.deleteRole(token, id); setMsg(t("Role deleted.", "تم الحذف.")); await load(); }
    catch (e) { setMsg((e as Error).message); }
  }

  async function createPerm() {
    if (!newPermName.trim()) return;
    try {
      await api.createPermission(token, newPermName, newPermDesc);
      setMsg(t("Permission created.", "تم إنشاء الصلاحية.")); setNewPermName(""); setNewPermDesc(""); await load();
    } catch (e) { setMsg((e as Error).message); }
  }

  async function deletePerm(id: string) {
    if (!confirm(t("Delete this permission?", "حذف هذه الصلاحية؟"))) return;
    try { await api.deletePermission(token, id); setMsg(t("Permission deleted.", "تم الحذف.")); await load(); }
    catch (e) { setMsg((e as Error).message); }
  }

  if (!canListRoles && !canListPerms) {
    return <p className="error">{t("You do not have access to this section.", "لا تملك صلاحية لهذا القسم.")}</p>;
  }

  return (
    <div>
      {msg && <p className="success-msg" style={{ marginBottom: "0.75rem" }}>{msg}</p>}
    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1.5rem", alignItems: "start" }}>
      {canListRoles && (
      <div>
        <h3 style={{ fontFamily: "'Cinzel',serif", fontSize: "1rem", color: "var(--gold-400)", marginBottom: "1rem" }}>
          {t("Roles", "الأدوار")}
        </h3>

        {canManageRoles && (
        <div className="card" style={{ marginBottom: "1rem", display: "flex", flexDirection: "column", gap: "0.75rem" }}>
          <p style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>
            {editRole ? t("Editing role:", "تعديل الدور:") + " " + editRole.name : t("New Role", "دور جديد")}
          </p>
          <div className="field">
            <label>{t("Name", "الاسم")}</label>
            <input value={newRoleName} onChange={(e) => setNewRoleName(e.target.value)} placeholder="e.g. Moderator" disabled={!!editRole?.isSystem} />
          </div>
          <div className="field">
            <label>{t("Description", "الوصف")}</label>
            <input value={newRoleDesc} onChange={(e) => setNewRoleDesc(e.target.value)} placeholder={t("Optional description", "وصف اختياري")} />
          </div>
          {canListPerms && (
            <div className="field">
              <label>{t("Permissions", "الصلاحيات")}</label>
              <div className="perm-grid">
                {perms.map((p) => (
                  <label key={p.id} className="perm-item">
                    <input type="checkbox" checked={newRolePerms.includes(p.name)} onChange={() => togglePerm(p.name)} />
                    {p.name}
                  </label>
                ))}
              </div>
            </div>
          )}
          <div className="row">
            <button className="btn btn-primary btn-sm" onClick={saveRole} type="button">
              {editRole ? t("Update", "تحديث") : t("Create", "إنشاء")}
            </button>
            {editRole && <button className="btn btn-secondary btn-sm" onClick={() => { setEditRole(null); setNewRoleName(""); setNewRoleDesc(""); setNewRolePerms([]); }} type="button">{t("Cancel", "إلغاء")}</button>}
          </div>
        </div>
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
          {roles.map((role) => (
            <div key={role.id} className="card" style={{ padding: "0.85rem 1rem" }}>
              <div className="row" style={{ justifyContent: "space-between", marginBottom: "0.4rem" }}>
                <div style={{ fontWeight: 600, color: "var(--text-primary)", fontSize: "0.9rem" }}>{role.name}</div>
                {canManageRoles && (
                <div className="row" style={{ gap: "0.4rem" }}>
                  {role.isSystem && <span className="chip" style={{ fontSize: "0.65rem" }}>{t("System", "نظام")}</span>}
                  <button className="btn btn-secondary btn-sm" onClick={() => startEdit(role)} type="button">{t("Edit", "تعديل")}</button>
                  <button className="btn btn-danger btn-sm" onClick={() => deleteRole(role.id, role.isSystem)} type="button">×</button>
                </div>
                )}
              </div>
              <div style={{ display: "flex", flexWrap: "wrap", gap: "0.3rem" }}>
                {role.permissions.map((p) => <span key={p} className="chip chip-gold" style={{ fontSize: "0.65rem" }}>{p}</span>)}
                {role.permissions.length === 0 && <span style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{t("No permissions", "لا صلاحيات")}</span>}
              </div>
            </div>
          ))}
        </div>
      </div>
      )}

      {canListPerms && (
      <div>
        <h3 style={{ fontFamily: "'Cinzel',serif", fontSize: "1rem", color: "var(--gold-400)", marginBottom: "1rem" }}>
          {t("Permissions", "الصلاحيات")}
        </h3>

        {canManagePerms && (
        <div className="card" style={{ marginBottom: "1rem", display: "flex", flexDirection: "column", gap: "0.75rem" }}>
          <p style={{ fontSize: "0.82rem", color: "var(--text-muted)" }}>{t("Create new permission", "إنشاء صلاحية جديدة")}</p>
          <div className="field">
            <label>{t("Name (slug)", "الاسم")}</label>
            <input value={newPermName} onChange={(e) => setNewPermName(e.target.value)} placeholder="e.g. approve.artwork" />
          </div>
          <div className="field">
            <label>{t("Description", "الوصف")}</label>
            <input value={newPermDesc} onChange={(e) => setNewPermDesc(e.target.value)} placeholder={t("Optional", "اختياري")} />
          </div>
          <button className="btn btn-primary btn-sm" onClick={createPerm} type="button">{t("Add Permission", "إضافة صلاحية")}</button>
        </div>
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "0.4rem" }}>
          {perms.map((p) => (
            <div key={p.id} className="card" style={{ padding: "0.75rem 1rem", display: "flex", alignItems: "center", justifyContent: "space-between" }}>
              <div>
                <div style={{ fontWeight: 600, color: "var(--text-primary)", fontSize: "0.85rem" }}>{p.name}</div>
                {p.description && <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{p.description}</div>}
              </div>
              {canManagePerms && <button className="btn btn-danger btn-sm" onClick={() => deletePerm(p.id)} type="button">×</button>}
            </div>
          ))}
        </div>
      </div>
      )}
    </div>
    </div>
  );
}

// ── Users tab ────────────────────────────────────────────────────
function UsersTab({ token, claims }: { token: string; claims: JwtClaims }) {
  const { t } = useLang();
  const canAssign = hasPermission(claims, "role.assignments.manage");
  const [users, setUsers] = useState<RbacUser[]>([]);
  const [roles, setRoles] = useState<RbacRole[]>([]);
  const [msg, setMsg] = useState("");

  const load = useCallback(async () => {
    const u = await api.getRbacUsers(token);
    setUsers(u);
    if (canAssign) {
      setRoles(await api.getRoles(token));
    }
  }, [canAssign, token]);

  useEffect(() => {
    load().catch(() => undefined);
  }, [load]);

  async function assign(userId: string, roleId: string) {
    try { await api.assignRoleToUser(token, userId, roleId); setMsg(t("Role assigned.", "تم تعيين الدور.")); await load(); }
    catch (e) { setMsg((e as Error).message); }
  }

  return (
    <div>
      {msg && <p className="success-msg" style={{ marginBottom: "0.75rem" }}>{msg}</p>}
      <div className="card" style={{ padding: 0, overflow: "hidden" }}>
        <table className="data-table">
          <thead>
            <tr>
              <th>{t("User", "المستخدم")}</th>
              <th>{t("Email", "البريد")}</th>
              <th>{t("Current Role", "الدور الحالي")}</th>
              <th>{t("Status", "الحالة")}</th>
              {canAssign && <th>{t("Assign Role", "تعيين دور")}</th>}
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td style={{ color: "var(--text-primary)", fontWeight: 500 }}>{u.username}</td>
                <td>{u.email}</td>
                <td><span className="chip chip-gold" style={{ fontSize: "0.72rem" }}>{u.role}</span></td>
                <td>
                  <span className={`badge ${u.isActive ? "badge--live" : "badge--ended"}`} style={{ fontSize: "0.65rem" }}>
                    {u.isActive ? t("Active", "نشط") : t("Inactive", "غير نشط")}
                  </span>
                </td>
                {canAssign && (
                <td>
                  <select
                    value=""
                    onChange={(e) => e.target.value && assign(u.id, e.target.value)}
                    style={{ fontSize: "0.78rem", padding: "0.3rem 0.5rem", width: "auto" }}
                  >
                    <option value="">{t("Change…", "تغيير…")}</option>
                    {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
                  </select>
                </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

type AdminTab = "artists" | "artworks" | "roles" | "users" | "catalog";

// ── Main AdminPage ───────────────────────────────────────────────
export function AdminPage() {
  const { token, claims } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();

  const TABS = useMemo(() => {
    const all: { key: AdminTab; label: string }[] = [
      { key: "artists", label: t("Pending Artists", "الفنانون المنتظرون") },
      { key: "artworks", label: t("Pending Artworks", "الأعمال المنتظرة") },
      { key: "roles", label: t("Roles & Permissions", "الأدوار والصلاحيات") },
      { key: "users", label: t("Users", "المستخدمون") },
      { key: "catalog", label: t("Categories & Tags", "التصنيفات والوسوم") },
    ];
    return all.filter((tb) => {
      if (tb.key === "artists") return hasAnyPermission(claims, PENDING_ARTIST_PERMS);
      if (tb.key === "artworks") return hasAnyPermission(claims, PENDING_ARTWORK_PERMS);
      if (tb.key === "roles") return hasAnyPermission(claims, ROLES_TAB_PERMS);
      if (tb.key === "users") return hasAnyPermission(claims, USERS_TAB_PERMS);
      if (tb.key === "catalog") return hasAnyPermission(claims, CATALOG_TAB_PERMS);
      return false;
    });
  }, [claims, t]);

  const [tab, setTab] = useState<AdminTab>("artists");
  useEffect(() => {
    if (TABS.length === 0) return;
    if (!TABS.some((x) => x.key === tab)) {
      setTab(TABS[0]!.key);
    }
  }, [TABS, tab]);

  if (!token) return <div className="container"><p className="error">{t("Unauthorized.", "غير مصرح.")}</p></div>;

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <h1 className="section-title">{t("Admin Dashboard", "لوحة الإدارة")}</h1>
      <p className="section-sub">
        {t("Sections match your permissions. Re-login if your role was updated.", "الأقسام تتبع صلاحياتك. سجّل دخولك من جديد إذا تغيّر دورك.")}
      </p>

      {TABS.length === 0 ? (
        <p className="error">{t("No admin sections for your account.", "لا توجد أقسام إدارية لحسابك.")}</p>
      ) : (
        <>
          <div className="tabs">
            {TABS.map((tb) => (
              <button key={tb.key} className={`tab-btn${tab === tb.key ? " active" : ""}`} onClick={() => setTab(tb.key)} type="button">
                {tb.label}
              </button>
            ))}
          </div>
          {tab === "artists" && <ArtistsTab token={token} />}
          {tab === "artworks" && <ArtworksTab token={token} />}
          {tab === "roles" && <RolesTab token={token} claims={claims} />}
          {tab === "users" && <UsersTab token={token} claims={claims} />}
          {tab === "catalog" && <CatalogTab token={token} />}
        </>
      )}
    </div>
  );
}
