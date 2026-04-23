import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useLang } from "../context/LangContext";
import { useGlobalToast } from "../context/ToastContext";
import { api } from "../lib/api";
import { IMAGE_PLACEHOLDER, resolveImageUrl } from "../lib/images";
import { buildAuctionConnection } from "../lib/signalr";
import { useCountdown } from "../lib/useCountdown";
import { ToastContainer } from "../components/Toast";
import { useToast } from "../lib/useToast";
import type { Artwork } from "../types";

type Category = { id: string; name: string; description: string };

function ArtworkCountdown({ endTime, startTime }: { endTime: string; startTime: string }) {
  const cd = useCountdown(endTime, startTime);
  if (cd.isExpired) return <span className="badge badge--ended">Ended</span>;
  if (cd.isUpcoming) return <span className="badge badge--upcoming">Upcoming</span>;
  return (
    <span className="badge badge--live">
      {cd.days > 0 ? `${cd.days}d ` : ""}
      {String(cd.hours).padStart(2, "0")}:{String(cd.minutes).padStart(2, "0")}:{String(cd.seconds).padStart(2, "0")}
    </span>
  );
}

function ArtworkCard({ artwork }: { artwork: Artwork }) {
  const { t } = useLang();
  const cd = useCountdown(artwork.auctionEndTime);
  const statusClass = cd.isExpired ? "badge--ended" : cd.isUpcoming ? "badge--upcoming" : "badge--live";

  return (
    <Link to={`/artworks/${artwork.id}`} className="artwork-card" style={{ textDecoration: "none" }}>
      <div className="artwork-card__img-wrap">
        <img
          src={resolveImageUrl(artwork.imageUrl)}
          alt={artwork.title}
          loading="lazy"
          onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }}
        />
        <div className="artwork-card__overlay">
          <span className="artwork-card__bid-live">
            {t("Current Bid", "المزايدة الحالية")} — ${artwork.currentBid || artwork.initialPrice}
          </span>
          <ArtworkCountdown endTime={artwork.auctionEndTime} startTime={""} />
        </div>
      </div>
      <div className="artwork-card__body">
        <div className="row" style={{ justifyContent: "space-between", alignItems: "flex-start" }}>
          <h3 className="artwork-card__title">{artwork.title}</h3>
          <span className={`badge ${statusClass}`} style={{ flexShrink: 0 }}>
            {cd.isExpired ? t("Ended", "انتهى") : cd.isUpcoming ? t("Soon", "قريباً") : t("Live", "مباشر")}
          </span>
        </div>
        <p className="artwork-card__meta">{artwork.artistName} · {artwork.categoryName}</p>
        <div className="artwork-card__footer">
          <span className="artwork-card__price">${artwork.currentBid || artwork.initialPrice}</span>
          <span className="artwork-card__cta">
            {artwork.status.toLowerCase() === "ended" ? t("View Details", "عرض التفاصيل") : t("Bid Now", "زايد الآن")}
          </span>
        </div>
      </div>
    </Link>
  );
}

export function ArtworksPage() {
  const { token } = useAuth();
  const { t } = useLang();
  const [searchParams] = useSearchParams();
  const [artworks, setArtworks] = useState<Artwork[]>([]);
  const [artistName, setArtistName] = useState("");
  const [categoryName, setCategoryName] = useState("");
  const [tagName, setTagName] = useState(searchParams.get("tagName") ?? "");
  const [categories, setCategories] = useState<Category[]>([]);
  const [tags, setTags] = useState<string[]>([]);
  const { toasts, push, dismiss } = useToast();
  const toast = useGlobalToast();
  const connectionRef = useRef<ReturnType<typeof buildAuctionConnection> | null>(null);

  async function load(artist = artistName, category = categoryName, tag = tagName) {
    const q = new URLSearchParams();
    if (artist) q.set("artistName", artist);
    if (category) q.set("categoryName", category);
    if (tag) q.set("tagName", tag);
    try {
      const result = await api.getArtworks(q.toString() ? `?${q.toString()}` : "");
      setArtworks(result.items);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed loading artworks.");
    }
  }

  useEffect(() => {
    load();
    // Load filter options
    Promise.all([api.getCategories(), api.getTags()])
      .then(([cats, tgs]) => { setCategories(cats); setTags(tgs); })
      .catch(() => undefined);
    // SignalR
    const conn = buildAuctionConnection(token ?? undefined);
    connectionRef.current = conn;
    conn.on("bid.placed", (ev: { artworkId: string; bidderName: string; amount: number }) => {
      push(`🔨 ${ev.bidderName} bid $${ev.amount}`, "bid");
      load();
    });
    conn.start().catch(() => undefined);
    return () => { conn.stop().catch(() => undefined); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  function onFilter(e: FormEvent) {
    e.preventDefault();
    load();
  }

  return (
    <>
      <ToastContainer toasts={toasts} dismiss={dismiss} />
      <div className="container">
        {/* Hero */}
        <div className="hero fade-up">
          <h1 className="hero-en">
            Discover <span className="hero-accent">Extraordinary</span> Art
          </h1>
          <p className="hero-ar">اكتشف تحف فنية استثنائية · زايد في الوقت الحقيقي</p>
          <p className="hero-sub">
            {t(
              "Bid on paintings, sculptures & digital art from world-class artists. Real-time auctions, secure transactions.",
              "زايد على لوحات ومنحوتات وفن رقمي من أمهر الفنانين. مزادات فورية وتحويلات آمنة.",
            )}
          </p>
        </div>

        {/* Filters */}
        <form className="filters-wrap" onSubmit={onFilter}>
          <div className="filters-grid">
            <div className="field">
              <label htmlFor="filter-artist">{t("Artist Name", "اسم الفنان")}</label>
              <input
                id="filter-artist"
                placeholder={t("e.g. Ahmed Ali", "مثال: أحمد علي")}
                value={artistName}
                onChange={(e) => setArtistName(e.target.value)}
              />
            </div>
            <div className="field">
              <label htmlFor="filter-category">{t("Category", "التصنيف")}</label>
              <select id="filter-category" value={categoryName} onChange={(e) => setCategoryName(e.target.value)}>
                <option value="">{t("All categories", "كل التصنيفات")}</option>
                {categories.map((c) => <option key={c.id} value={c.name}>{c.name}</option>)}
              </select>
            </div>
            <div className="field">
              <label htmlFor="filter-tag">{t("Tag", "الوسم")}</label>
              <select id="filter-tag" value={tagName} onChange={(e) => setTagName(e.target.value)}>
                <option value="">{t("All tags", "كل الوسوم")}</option>
                {tags.map((tag) => <option key={tag} value={tag}>{tag}</option>)}
              </select>
            </div>
          </div>
          <div className="filters-actions">
            <button type="submit" className="btn btn-primary btn-sm">
              {t("Apply Filters", "تطبيق الفلاتر")}
            </button>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => { setArtistName(""); setCategoryName(""); setTagName(""); load("", "", ""); }}
            >
              {t("Clear", "مسح")}
            </button>
          </div>
        </form>

        <p className="section-sub">
          {artworks.length} {t("artworks available", "عمل فني متاح")}
        </p>

        <div className="grid">
          {artworks.map((artwork) => (
            <ArtworkCard key={artwork.id} artwork={artwork} />
          ))}
        </div>

        {artworks.length === 0 && (
          <div style={{ textAlign: "center", padding: "3rem", color: "var(--text-muted)" }}>
            <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>🎨</div>
            <p>{t("No artworks found. Try adjusting your filters.", "لا توجد أعمال. جرب تغيير الفلاتر.")}</p>
          </div>
        )}
      </div>
    </>
  );
}
