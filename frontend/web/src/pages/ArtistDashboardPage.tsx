import { useEffect, useMemo, useRef, useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";
import { api } from "../lib/api";
import { IMAGE_PLACEHOLDER, resolveImageUrl } from "../lib/images";
import type { Artwork, CreateArtworkInput } from "../types";

type FormState = {
  title: string; description: string; initialPrice: string; buyNowPrice: string;
  auctionStartTime: string; auctionEndTime: string; categoryId: string; categoryName: string; tags: string[]; imageUrl: string;
};

const EMPTY: FormState = {
  title: "", description: "", initialPrice: "", buyNowPrice: "",
  auctionStartTime: "", auctionEndTime: "", categoryId: "", categoryName: "", tags: [], imageUrl: "",
};

function toIso(v: string) { return new Date(v).toISOString(); }

function validate(f: FormState): string {
  const init = Number(f.initialPrice);
  const bn   = f.buyNowPrice ? Number(f.buyNowPrice) : null;
  const s    = new Date(f.auctionStartTime);
  const e    = new Date(f.auctionEndTime);
  if (isNaN(init) || init <= 0) return "Initial price must be > 0.";
  if (bn !== null && (isNaN(bn) || bn < init)) return "Buy-now price must be ≥ initial price.";
  if (isNaN(s.getTime()) || isNaN(e.getTime())) return "Start and end time are required.";
  if (e <= s) return "Auction end must be after start.";
  if (!f.imageUrl.trim()) return "Image is required (upload or paste URL).";
  if (f.tags.length === 0) return "Select at least one tag.";
  return "";
}

export function ArtistDashboardPage() {
  const { token, user } = useAuth();
  const { t } = useLang();
  const navigate = useNavigate();
  const toast = useGlobalToast();
  const [items, setItems] = useState<Artwork[]>([]);
  const [form, setForm] = useState<FormState>(EMPTY);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [extendVal, setExtendVal] = useState<Record<string, string>>({});
  const [categories, setCategories] = useState<{ id: string; name: string; description: string }[]>([]);
  const [availableTags, setAvailableTags] = useState<string[]>([]);
  const [uploading, setUploading] = useState(false);
  const [valErr, setValErr] = useState("");
  const formRef = useRef<HTMLFormElement>(null);

  const canLoad = useMemo(() => !!token && !!user?.id, [token, user?.id]);

  async function loadMine() {
    if (!token || !user?.id) return;
    const r = await api.getArtworksByArtist(user.id, token);
    setItems(r.items);
  }

  useEffect(() => {
    if (!canLoad) return;
    Promise.all([loadMine(), api.getCategories(), api.getTags()])
      .then(([, cats, tags]) => { setCategories(cats); setAvailableTags(tags); })
      .catch((e) => toast.error(e.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canLoad]);

  function toPayload(f: FormState): CreateArtworkInput {
    return {
      title: f.title.trim(), description: f.description.trim(),
      initialPrice: Number(f.initialPrice),
      buyNowPrice: f.buyNowPrice ? Number(f.buyNowPrice) : null,
      auctionStartTime: toIso(f.auctionStartTime),
      auctionEndTime:   toIso(f.auctionEndTime),
      categoryName: f.categoryName.trim(),
      tags: f.tags,
      imageUrl: f.imageUrl.trim(),
    };
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    if (!token) return;
    setValErr("");
    const v = validate(form);
    if (v) { setValErr(v); return; }
    try {
      if (editingId) {
        await api.updateArtwork(token, editingId, toPayload(form));
        toast.success(t("Artwork updated.", "تم تحديث العمل."));
      } else {
        await api.createArtwork(token, toPayload(form));
        toast.success(t("Artwork submitted for review.", "تم إرسال العمل للمراجعة."));
      }
      setEditingId(null); setForm(EMPTY); await loadMine();
    } catch (e) { toast.error((e as Error).message); }
  }

  async function onDelete(id: string) {
    if (!token || !confirm(t("Delete this artwork?", "حذف هذا العمل؟"))) return;
    try { await api.deleteArtwork(token, id); toast.success(t("Deleted.", "تم الحذف.")); await loadMine(); }
    catch (e) { toast.error((e as Error).message); }
  }

  async function onExtend(id: string) {
    if (!token || !extendVal[id]) return;
    try {
      await api.extendAuction(token, id, toIso(extendVal[id]));
      toast.success(t("Auction extended.", "تم تمديد المزاد."));
      setExtendVal((p) => ({ ...p, [id]: "" }));
      await loadMine();
    } catch (e) { toast.error((e as Error).message); }
  }

  async function onFileUpload(file: File | null) {
    if (!file || !token) return;
    setUploading(true);
    try {
      const r = await api.uploadImage(token, file);
      setForm((p) => ({ ...p, imageUrl: r.imageUrl }));
    } catch (e) { toast.error((e as Error).message); }
    finally { setUploading(false); }
  }

  function startEdit(item: Artwork) {
    setEditingId(item.id);
    setForm({ ...EMPTY, title: item.title, categoryName: item.categoryName, categoryId: "", initialPrice: String(item.initialPrice), imageUrl: item.imageUrl });
    formRef.current?.scrollIntoView({ behavior: "smooth" });
  }

  function statusBadge(status: string) {
    const map: Record<string, string> = {
      Pending: "badge--pending", Approved: "badge--live",
      Live: "badge--live", Ended: "badge--ended", Rejected: "badge--ended",
    };
    return `badge ${map[status] ?? "badge--upcoming"}`;
  }

  return (
    <div className="container">
      <button className="nav-link" onClick={() => navigate(-1)} style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "0.4rem" }}>
        {t("← Back", "← عودة")}
      </button>
      <h1 className="section-title">{t("Artist Dashboard", "لوحة الفنان")}</h1>
      <p className="section-sub">{t("Create and manage your artworks. New posts require admin approval.", "أنشئ وأدر أعمالك الفنية. تتطلب الموافقة.")}</p>

      {/* Form */}
      <form ref={formRef} className="card" style={{ marginBottom: "2rem", display: "flex", flexDirection: "column", gap: "1rem" }} onSubmit={onSubmit}>
        <h2 style={{ fontFamily: "'Cinzel', serif", fontSize: "1.1rem", color: "var(--gold-400)" }}>
          {editingId ? t("✏️ Edit Artwork", "✏️ تعديل العمل") : t("➕ Create Artwork", "➕ إنشاء عمل")}
        </h2>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem" }}>
          <div className="field">
            <label htmlFor="art-title">{t("Title", "العنوان")}</label>
            <input id="art-title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder={t("Artwork title", "عنوان العمل")} required />
          </div>
          <div className="field">
            <label htmlFor="art-category">{t("Category", "التصنيف")}</label>
            <select
              id="art-category"
              value={form.categoryName}
              onChange={(e) => {
                const selected = categories.find((c) => c.name === e.target.value);
                setForm({ ...form, categoryName: e.target.value, categoryId: selected?.id ?? "" });
              }}
              required
            >
              <option value="">{t("-- Select category --", "-- اختر تصنيفاً --")}</option>
              {categories.map((c) => <option key={c.id} value={c.name}>{c.name}</option>)}
            </select>
          </div>
        </div>

        <div className="field">
          <label htmlFor="art-desc">{t("Description", "الوصف")}</label>
          <textarea id="art-desc" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} placeholder={t("Describe your artwork…", "صف عملك الفني…")} required />
        </div>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem" }}>
          <div className="field">
            <label htmlFor="art-init">{t("Initial Price (USD)", "السعر الابتدائي")}</label>
            <input id="art-init" type="number" value={form.initialPrice} onChange={(e) => setForm({ ...form, initialPrice: e.target.value })} placeholder="e.g. 150" required />
          </div>
          <div className="field">
            <label htmlFor="art-buynow">{t("Buy Now Price (optional)", "اشتري الآن (اختياري)")}</label>
            <input id="art-buynow" type="number" value={form.buyNowPrice} onChange={(e) => setForm({ ...form, buyNowPrice: e.target.value })} placeholder="e.g. 500" />
          </div>
        </div>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem" }}>
          <div className="field">
            <label htmlFor="art-start">{t("Auction Start", "بداية المزاد")}</label>
            <input id="art-start" type="datetime-local" value={form.auctionStartTime} onChange={(e) => setForm({ ...form, auctionStartTime: e.target.value })} required />
          </div>
          <div className="field">
            <label htmlFor="art-end">{t("Auction End", "نهاية المزاد")}</label>
            <input id="art-end" type="datetime-local" value={form.auctionEndTime} onChange={(e) => setForm({ ...form, auctionEndTime: e.target.value })} required />
          </div>
        </div>

        <div className="field">
          <label>{t("Tags", "الوسوم")}</label>
          <div className="perm-grid">
            {availableTags.map((tag) => (
              <label key={tag} className="perm-item">
                <input
                  type="checkbox"
                  checked={form.tags.includes(tag)}
                  onChange={(e) => {
                    const next = e.target.checked
                      ? [...form.tags, tag]
                      : form.tags.filter((t) => t !== tag);
                    setForm({ ...form, tags: next });
                  }}
                />
                {tag}
              </label>
            ))}
          </div>
          {form.tags.length > 0 && (
            <div style={{ display: "flex", flexWrap: "wrap", gap: "0.3rem", marginTop: "0.4rem" }}>
              {form.tags.map((tag) => <span key={tag} className="chip chip-gold">{tag}</span>)}
            </div>
          )}
        </div>

        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem", alignItems: "start" }}>
          <div className="field">
            <label htmlFor="art-img-url">{t("Image URL", "رابط الصورة")}</label>
            <input id="art-img-url" value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} placeholder="https://..." />
          </div>
          <div className="field">
            <label htmlFor="art-img-file">{t("Or Upload Image", "أو ارفع صورة")}</label>
            <input id="art-img-file" type="file" accept="image/*" onChange={(e) => onFileUpload(e.target.files?.[0] ?? null)} />
          </div>
        </div>

        {form.imageUrl && (
          <img className="img-preview" src={resolveImageUrl(form.imageUrl)} alt="preview" onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }} />
        )}
        {uploading && <p style={{ color: "var(--text-muted)", fontSize: "0.85rem" }}>{t("Uploading…", "جاري الرفع…")}</p>}
        {valErr && <p className="error">{valErr}</p>}

        <div className="form-actions">
          <button type="submit" className="btn btn-primary">
            {editingId ? t("Update Artwork", "تحديث العمل") : t("Create Artwork", "إنشاء عمل")}
          </button>
          {editingId && (
            <button type="button" className="btn btn-secondary" onClick={() => { setEditingId(null); setForm(EMPTY); }}>
              {t("Cancel", "إلغاء")}
            </button>
          )}
        </div>
      </form>

      {/* Artwork list */}
      <h2 style={{ fontFamily: "'Cinzel', serif", fontSize: "1.1rem", color: "var(--text-primary)", marginBottom: "1rem" }}>
        {t("My Artworks", "أعمالي")} ({items.length})
      </h2>
      <div className="grid">
        {items.map((item) => (
          <article key={item.id} className="artwork-card">
            <div className="artwork-card__img-wrap">
              <img src={resolveImageUrl(item.imageUrl)} alt={item.title} onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }} />
            </div>
            <div className="artwork-card__body">
              <div className="row" style={{ justifyContent: "space-between" }}>
                <h3 className="artwork-card__title">{item.title}</h3>
                <span className={statusBadge(item.status)}>{item.status}</span>
              </div>
              <p className="artwork-card__meta">{item.categoryName}</p>
              <p style={{ fontSize: "0.82rem", color: "var(--gold-400)", fontWeight: 700 }}>${item.currentBid || item.initialPrice}</p>
              <div className="row" style={{ marginTop: "0.5rem" }}>
                <button className="btn btn-secondary btn-sm" onClick={() => startEdit(item)} type="button">{t("Edit", "تعديل")}</button>
                <button className="btn btn-danger btn-sm" onClick={() => onDelete(item.id)} type="button">{t("Delete", "حذف")}</button>
              </div>
              <div className="row" style={{ marginTop: "0.4rem" }}>
                <input
                  type="datetime-local"
                  style={{ flex: 1, fontSize: "0.78rem", padding: "0.4rem 0.6rem" }}
                  value={extendVal[item.id] ?? ""}
                  onChange={(e) => setExtendVal((p) => ({ ...p, [item.id]: e.target.value }))}
                />
                <button className="btn btn-secondary btn-sm" onClick={() => onExtend(item.id)} type="button">
                  {t("Extend", "تمديد")}
                </button>
              </div>
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
